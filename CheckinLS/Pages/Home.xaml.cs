using Acr.UserDialogs;
using CheckinLS.API;
using Plugin.NFC;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CheckinLS.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Home
    {
        private static MainSql _sql;
        private bool _fakeListener, _busy, _disableNfcError, _startup = true;
        private (bool curs, bool pregatire, bool recuperare) _ora = (false, false, false);
        private static int _index;

        public Home(MainSql sql)
        {
            InitializeComponent();

            _sql = sql;

            LeftButton.Clicked += LeftButton_Clicked;
            RightButton.Clicked += RightButton_Clicked;

            DeleteButton.Clicked += DeleteButton_Clicked;
            ManualAddButton.Clicked += ManualAddButton_Clicked;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            ObsEntry.Text = "";

            RefreshPage();

            if (_startup)
                await NfcServiceAsync();
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
            if (_index > 0)
            {
                --_index;
                RefreshPage();
            }
        }

        private void RightButton_Clicked(object sender, EventArgs e)
        {
            if (_index < _sql.MaxElement() - 1)
            {
                ++_index;
                RefreshPage();
            }
        }

        private async void DeleteButton_Clicked(object sender, EventArgs e)
        {
            if (_sql.MaxElement() == 0 || !IdLabel.Text.All(char.IsDigit))
                return;

            var result = await DisplayAlert("Alert!", "Are you sure you want to delete the entry?", "Yes", "No");

            if (!result)
            {
                Analytics.TrackEvent("Delete entry cancelled");
                return;
            }

            Analytics.TrackEvent("Entry deleted");

            DeleteButton.IsEnabled = false;

            await _sql.DeleteFromDbAsync(Convert.ToInt32(IdLabel.Text));

            if (_index > 0 && _index > _sql.MaxElement() - 1)
                --_index;
            RefreshPage();

            ShowToast("Entry deleted!");

            DeleteButton.IsEnabled = true;
        }

        private void ManualAddButton_Clicked(object sender, EventArgs e) =>
                Navigation.PushModalAsync(new ManualAdd());

        public static async Task AddNewEntryExternalAsync(string observatii, bool curs, bool pregatire, bool recuperare)
        {
            await _sql.AddNewEntryInDbAsync(observatii == string.Empty ? "None" : observatii?.ToUpperInvariant(), curs, pregatire, recuperare);
            _index = _sql.MaxElement() - 1;
            ShowToast("New entry added!");
        }

        private async Task AddNewEntryAsync(string observatii = "", bool curs = false, bool pregatire = false, bool recuperare = false)
        {
            await AddNewEntryExternalAsync(observatii, curs, pregatire, recuperare);
            ObsEntry.Text = "";
            RefreshPage();
        }

        private void RefreshPage()
        {
            if (_sql.MaxElement() == 0)
            {
                IdLabel.Text = Observatii.Text = Date.Text = OraIncepere.Text = OraSfarsit.Text =
                   CursAlocat.Text = PregatireAlocat.Text = RecuperareAlocat.Text =
                   Total.Text = "Not found!";

                PretTotal.Text = "0";

                return;
            }

            (IdLabel.Text, Observatii.Text, Date.Text, OraIncepere.Text, OraSfarsit.Text, CursAlocat.Text, PregatireAlocat.Text,
                    RecuperareAlocat.Text, Total.Text) =
                (ConversionWrapper((int)_sql.Elements["id"][_index]),
                    ConversionWrapper((string)_sql.Elements["observatii"][_index]),
                    ConversionWrapper((DateTime)_sql.Elements["date"][_index]),
                    ConversionWrapper((TimeSpan)_sql.Elements["ora_incepere"][_index]),
                    ConversionWrapper((TimeSpan)_sql.Elements["ora_final"][_index]),
                    ConversionWrapper((TimeSpan)_sql.Elements["curs_alocat"][_index]),
                    ConversionWrapper((TimeSpan)_sql.Elements["pregatire_alocat"][_index]),
                    ConversionWrapper((TimeSpan)_sql.Elements["recuperare_alocat"][_index]),
                    ConversionWrapper((TimeSpan)_sql.Elements["total"][_index]));

            SetPrice();
        }

        private async Task NfcServiceAsync()
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
            var zero = TimeSpan.FromHours(0);
            var total = (curs: zero, pregatire: zero, recuperare: zero);

            foreach (var time in _sql.Elements["curs_alocat"])
            {
                total.curs += (TimeSpan)time;
            }

            foreach (var time in _sql.Elements["pregatire_alocat"])
            {
                total.pregatire += (TimeSpan)time;
            }

            foreach (var time in _sql.Elements["recuperare_alocat"])
            {
                total.recuperare += (TimeSpan)time;
            }

            var valoare = GetIndice(total.curs) * Constants.PretCurs + GetIndice(total.pregatire) * Constants.PretPregatire + GetIndice(total.recuperare) * Constants.PretRecuperare;

            PretTotal.Text = valoare.ToString(CultureInfo.InvariantCulture);
        }

        private async Task WaitAndAddAsync()
        {
            _busy = true;

            await CountdownAsync();
            await AddNewEntryAsync(ObsEntry.Text, _ora.curs, _ora.pregatire,
                _ora.recuperare);

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

        private static double GetIndice(TimeSpan time) =>
                (DateTime.Parse(time.ToString(@"hh\:mm")) - DateTime.Parse("00:00")).TotalHours;

        private static void ShowToast(string message) =>
                Device.BeginInvokeOnMainThread(() =>
                    UserDialogs.Instance.Toast(message));
    }
}