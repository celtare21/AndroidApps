using Acr.UserDialogs;
using CheckinLS.API;
using CheckinLS.InterfacesAndClasses;
using Microsoft.AppCenter.Analytics;
using Plugin.NFC;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CheckinLS.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Home
    {
        private Elements _elements;
        private bool _fakeListener, _busy, _disableNfcError, _startup = true;
        private (bool curs, bool pregatire, bool recuperare) _ora = (false, false, false);

        public Home()
        {
            InitializeComponent();

            LeftButton.Clicked += LeftButton_Clicked;
            RightButton.Clicked += RightButton_Clicked;
            
            DeleteButton.Clicked += DeleteButton_Clicked;
            ManualAddButton.Clicked += ManualAddButton_Clicked;
        }

        public async Task CreateElementsAsync(MainSql sqlClass) =>
                _elements = await Elements.CreateAsync(sqlClass, new GetDate());

        protected override bool OnBackButtonPressed()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                var result = await DisplayAlert("Alert!", "Do you really want to exit the application?", "Yes", "No");

                if (result)
                    App.Close();
            });

            return true;
        }

        private void LeftButton_Clicked(object sender, EventArgs e)
        {
            if (_elements == null || _elements.Index <= 0)
                return;

            --_elements.Index;
            RefreshPage();
        }

        private void RightButton_Clicked(object sender, EventArgs e)
        {
            if (_elements == null || _elements.Index >= _elements.MaxElement() - 1)
                return;

            ++_elements.Index;
            RefreshPage();
        }

        private async void DeleteButton_Clicked(object sender, EventArgs e)
        {
            if (_elements == null || _elements.MaxElement() == 0 || !IdLabel.Text.All(char.IsDigit))
                return;

            var result = await DisplayAlert("Alert!", "Are you sure you want to delete the entry?", "Yes", "No");

            if (!result)
            {
                Analytics.TrackEvent("Delete entry cancelled");
                return;
            }

            Analytics.TrackEvent("Entry deleted");

            DeleteButton.IsEnabled = false;

            await _elements.DeleteEntryAsync(Convert.ToInt32(IdLabel.Text));

            if (_elements.Index > 0 && _elements.Index > _elements.MaxElement() - 1)
                --_elements.Index;
            RefreshPage();

            ShowToast("Entry deleted!");

            DeleteButton.IsEnabled = true;
        }

        private void ManualAddButton_Clicked(object sender, EventArgs e) =>
                Navigation.PushModalAsync(new ManualAdd(_elements, this));

        public void RefreshPage()
        {
            if (_elements == null || _elements.MaxElement() == 0)
            {
                IdLabel.Text = Observatii.Text = Date.Text = OraIncepere.Text = OraSfarsit.Text =
                   CursAlocat.Text = PregatireAlocat.Text = RecuperareAlocat.Text =
                   Total.Text = "Not found!";

                PretTotal.Text = "0";

                return;
            }

            (IdLabel.Text, Observatii.Text, Date.Text, OraIncepere.Text, OraSfarsit.Text, CursAlocat.Text, PregatireAlocat.Text,
                    RecuperareAlocat.Text, Total.Text) =
                (ConversionWrapper(_elements.Entries[_elements.Index].Id),
                    ConversionWrapper(_elements.Entries[_elements.Index].Observatii),
                    ConversionWrapper(_elements.Entries[_elements.Index].Date),
                    ConversionWrapper(_elements.Entries[_elements.Index].OraIncepere),
                    ConversionWrapper(_elements.Entries[_elements.Index].OraFinal),
                    ConversionWrapper(_elements.Entries[_elements.Index].CursAlocat),
                    ConversionWrapper(_elements.Entries[_elements.Index].PregatireAlocat),
                    ConversionWrapper(_elements.Entries[_elements.Index].RecuperareAlocat),
                    ConversionWrapper(_elements.Entries[_elements.Index].Total));

            SetPrice();
        }

        public async Task NfcServiceAsync()
        {
            if (!CrossNFC.IsSupported)
            {
                if (!_disableNfcError)
                {
                    await DisplayAlert("Error", "NFC is not supported! Please manually add new lessons.", "OK");
                    _disableNfcError = true;
                }
                return;
            }

            if (!CrossNFC.Current.IsAvailable)
            {
                if (!_disableNfcError)
                {
                    await DisplayAlert("Error", "NFC is not available", "OK");
                    _disableNfcError = true;
                }
                return;
            }

            if (!CrossNFC.Current.IsEnabled)
            {
                if (!_disableNfcError)
                {
                    await DisplayAlert("Error", "NFC is disabled", "OK");
                    _disableNfcError = true;
                }

                RegisterNfsStatusListener();
                return;
            }

            StartListening();
        }

        private void RegisterNfsStatusListener() =>
                CrossNFC.Current.OnNfcStatusChanged += Current_OnNfcStatusChanged;

        private async void Current_OnNfcStatusChanged(bool isEnabled)
        {
            if (isEnabled)
            {
                CrossNFC.Current.OnNfcStatusChanged -= Current_OnNfcStatusChanged;

                await NfcServiceAsync();
            }
        }

        private void StartListening() =>
                Device.BeginInvokeOnMainThread(() =>
                {
                    SubscribeEventsReal();

                    if (_startup)
                    {
                        Indicator.Color = Color.Green;

                        CrossNFC.Current.StartListening();
                        _startup = false;
                        _disableNfcError = false;
                    }
                });

        private void StopListening() =>
                Device.BeginInvokeOnMainThread(SubscribeFake);

        private void SubscribeEventsReal()
        {
            if (_fakeListener)
                CrossNFC.Current.OnMessageReceived -= Current_OnMessageReceivedFake;
            CrossNFC.Current.OnMessageReceived += Current_OnMessageReceived;
            _fakeListener = false;
        }

        private void SubscribeFake()
        {
            CrossNFC.Current.OnMessageReceived -= Current_OnMessageReceived;
            CrossNFC.Current.OnMessageReceived += Current_OnMessageReceivedFake;
            _fakeListener = true;
        }

        private async void Current_OnMessageReceived(ITagInfo tagInfo)
        {
            if (_elements == null)
                return;

            StopListening();

            if (tagInfo == null)
            {
                await DisplayAlert("Error", "No tag found", "OK");
                return;
            }

            if (!tagInfo.IsSupported || tagInfo.IsEmpty)
            {
                await DisplayAlert("Error", "Unsupported/Empty tag", "OK");
                return;
            }

            _ = FlashColorAsync();

            switch (GetMessage(tagInfo.Records[0]))
            {
                case "adauga_ora_curs":
                    _ora.curs = true;
                    break;
                case "adauga_ora_pregatire":
                    _ora.pregatire = true;
                    break;
                case "adauga_ora_recuperare":
                    _ora.recuperare = true;
                    break;
            }

            Analytics.TrackEvent("NFC tag read");

            if (!_busy)
                _ = WaitAndAddAsync();

            StartListening();
        }

        private void SetPrice()
        {
            var total = (curs: 0.0, pregatire: 0.0, recuperare: 0.0);

            foreach (var elem in _elements.Entries)
            {
                total.curs += elem.CursAlocat.TotalHours;
                total.pregatire += elem.PregatireAlocat.TotalHours;
                total.recuperare += elem.RecuperareAlocat.TotalHours;
            }

            var valoare = total.curs * Constants.PretCurs + total.pregatire * Constants.PretPregatire + total.recuperare * Constants.PretRecuperare;

            PretTotal.Text = valoare.ToString(CultureInfo.InvariantCulture);
        }

        private async Task WaitAndAddAsync()
        {
            _busy = true;

            await CountdownAsync();

            await _elements.AddNewEntryAsync(ObsEntry.Text, _ora.curs, _ora.pregatire, _ora.recuperare);
            ShowToast("New entry added!");
            ObsEntry.Text = "";
            RefreshPage();

            (_ora.curs, _ora.pregatire, _ora.recuperare) = (false, false, false);

            _busy = false;
        }

        private async Task CountdownAsync()
        {
            UserDialogs.Instance.ShowLoading("Waiting...");
            await Task.Delay(6000).ConfigureAwait(false);
            UserDialogs.Instance.HideLoading();
            await Task.Delay(100).ConfigureAwait(false);
        }

        private async Task FlashColorAsync()
        {
            Indicator.Color = Color.Red;
            await Task.Delay(500);
            Indicator.Color = Color.Green;
        }

        private void Current_OnMessageReceivedFake(ITagInfo tagInfo)
        {
            // Do nothing;
        }

        private string GetMessage(NFCNdefRecord record)
        {
            if (record.TypeFormat != NFCNdefTypeFormat.WellKnown ||
                string.IsNullOrWhiteSpace(record.MimeType) && record.MimeType != "text/plain")
                return null;

            return record.Message;
        }

        private string ConversionWrapper<T>(T elem) =>
            elem switch
            {
                int i => i.ToString(),
                string str => str,
                DateTime time => time.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                TimeSpan span => span.ToString(@"hh\:mm"),
                _ => throw new ArgumentException()
            };

        public static void ShowAlertKill(string message) =>
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Application.Current.MainPage.DisplayAlert("Error", message, "OK");
                Analytics.TrackEvent("App crashed");
                App.Close();
            });

        public void ShowToast(string message) =>
                Device.BeginInvokeOnMainThread(() =>
                    UserDialogs.Instance.Toast(message));
    }
}