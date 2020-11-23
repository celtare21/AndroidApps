using Acr.UserDialogs;
using CheckinLS.API.Misc;
using CheckinLS.API.Standard;
using CheckinLS.InterfacesAndClasses.Date;
using Microsoft.AppCenter.Analytics;
using Plugin.NFC;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using MainSql = CheckinLS.API.Sql.MainSql;

// ReSharper disable RedundantCapturedContext
namespace CheckinLS.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Home
    {
#pragma warning disable CS0649
        private StandardElements _elements;
#pragma warning restore CS0649
        private bool _fakeListener, _busy, _disableNfcError, _startup = true;
        private (bool curs, bool pregatire, bool recuperare) _ora = (false, false, false);

        public Home()
        {
            InitializeComponent();
        }

        public async Task CreateElementsAsync()
        {
            if (MainSql.UserHasOffice())
                OreOfficeButton.IsVisible = true;

            _elements = await StandardElements.CreateAsync(new GetDate());
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            AddEvents();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            RemoveEvents();
        }

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

            HelperFunctions.ShowToast("Entry deleted!");

            DeleteButton.IsEnabled = true;
        }

        private void ManualAddButton_Clicked(object sender, EventArgs e) =>
                Navigation.PushModalAsync(new HomeAddPage(_elements, this));

        private async void OreOfficeButton_Clicked(object sender, EventArgs e)
        {
            var officeClass = new OfficeHomePage();
            await Navigation.PushModalAsync(officeClass);
            await officeClass.CreateElementsAsync();
            officeClass.RefreshPage();
        }

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
                (HelperFunctions.ConversionWrapper(_elements.Entries[_elements.Index].Id),
                    HelperFunctions.ConversionWrapper(_elements.Entries[_elements.Index].Observatii),
                    HelperFunctions.ConversionWrapper(_elements.Entries[_elements.Index].Date),
                    HelperFunctions.ConversionWrapper(_elements.Entries[_elements.Index].OraIncepere),
                    HelperFunctions.ConversionWrapper(_elements.Entries[_elements.Index].OraFinal),
                    HelperFunctions.ConversionWrapper(_elements.Entries[_elements.Index].CursAlocat),
                    HelperFunctions.ConversionWrapper(_elements.Entries[_elements.Index].PregatireAlocat),
                    HelperFunctions.ConversionWrapper(_elements.Entries[_elements.Index].RecuperareAlocat),
                    HelperFunctions.ConversionWrapper(_elements.Entries[_elements.Index].Total));

            SetPrice();
        }

        private void AddEvents()
        {
            LeftButton.Clicked += LeftButton_Clicked;
            RightButton.Clicked += RightButton_Clicked;

            DeleteButton.Clicked += DeleteButton_Clicked;
            ManualAddButton.Clicked += ManualAddButton_Clicked;

            OreOfficeButton.Clicked += OreOfficeButton_Clicked;

            RegisterNfsStatusListener();
            StartListening();
        }

        private void RemoveEvents()
        {
            LeftButton.Clicked -= LeftButton_Clicked;
            RightButton.Clicked -= RightButton_Clicked;

            DeleteButton.Clicked -= DeleteButton_Clicked;
            ManualAddButton.Clicked -= ManualAddButton_Clicked;

            OreOfficeButton.Clicked -= OreOfficeButton_Clicked;
            
            RemoveNfsStatusListener();
            StopListening();
        }

        public async Task CheckNfcStatusAsync()
        {
            if (!CrossNFC.Current.IsEnabled)
            {
                Indicator.Color = Color.Gray;

                if (!_disableNfcError)
                {
                    await DisplayAlert("Error", "NFC is disabled", "OK");
                    _disableNfcError = true;
                }
            }
            else
            {
                Indicator.Color = Color.Green;
            }
        }

        private void RegisterNfsStatusListener() =>
                CrossNFC.Current.OnNfcStatusChanged += Current_OnNfcStatusChanged;

        private void RemoveNfsStatusListener() =>
                CrossNFC.Current.OnNfcStatusChanged -= Current_OnNfcStatusChanged;

        private async void Current_OnNfcStatusChanged(bool isEnabled)
        {
            if (!isEnabled)
            {
                Indicator.Color = Color.Gray;
                if (!_disableNfcError)
                {
                    await DisplayAlert("Error", "NFC is disabled", "OK");
                    _disableNfcError = true;
                }
            }
            else
            {
                Indicator.Color = Color.Green;
            }
        }

        private void StartListening() =>
                Device.BeginInvokeOnMainThread(() =>
                {
                    SubscribeEventsReal();

                    if (_startup)
                    {
                        CrossNFC.Current.StartListening();
                        _startup = false;
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
            if (_elements == null)
                return;

            var valoare = _elements.Entries.Sum(hours => hours.CursAlocat.TotalHours) * Constants.PretCurs +
                          _elements.Entries.Sum(hours => hours.PregatireAlocat.TotalHours) * Constants.PretPregatire +
                          _elements.Entries.Sum(hours => hours.RecuperareAlocat.TotalHours) * Constants.PretRecuperare;

            PretTotal.Text = valoare.ToString(CultureInfo.InvariantCulture);
        }

        private async Task WaitAndAddAsync()
        {
            _busy = true;

            await CountdownAsync();

            await _elements.AddNewEntryAsync(ObsEntry.Text, _ora.curs, _ora.pregatire, _ora.recuperare);
            HelperFunctions.ShowToast("New entry added!");
            ObsEntry.Text = "";
            RefreshPage();

            _ora = (false, false, false);

            _busy = false;
        }

        private async Task CountdownAsync()
        {
            UserDialogs.Instance.ShowLoading("Waiting...");
            for (int i = 0; i < 12 && _ora != (true, true, true); i++)
                await Task.Delay(500);
            UserDialogs.Instance.HideLoading();
            await Task.Delay(100);
        }

        private async Task FlashColorAsync()
        {
            Indicator.Color = Color.Red;
            await Task.Delay(500);
            Indicator.Color = Color.Green;
        }

        private static void Current_OnMessageReceivedFake(ITagInfo tagInfo)
        {
            // Do nothing;
        }

        private static string GetMessage(NFCNdefRecord record)
        {
            if (record.TypeFormat != NFCNdefTypeFormat.WellKnown ||
                string.IsNullOrWhiteSpace(record.MimeType) && record.MimeType != "text/plain")
                return null;

            return record.Message;
        }
    }
}