using Android.Widget;
using CheckinLS.API;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Acr.UserDialogs;
using Plugin.NFC;

namespace CheckinLS.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Home : ContentPage
    {
        private static MainSQL Sql;
        public static string Name;
        private bool FakeListener, Busy, Startup = true;
        private (bool curs, bool pregatire, bool recuperare) Ora = (false, false, false);
        private static int Index = 0;

        public Home(string name, MainSQL sql)
        {
            InitializeComponent();

            Name = name;
            Sql = sql;

            left_button.Clicked += Left_button_Clicked;
            right_button.Clicked += Right_button_Clicked;

            delete_button.Clicked += Delete_button_Clicked;
            manual_add_button.Clicked += Manual_add_button_Clicked;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            RefreshPage();

            if (Startup)
                await NfcService();
        }

        protected override bool OnBackButtonPressed()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                var result = await DisplayAlert("Alert!", "Do you really want to exit the application?", "Yes", "No");

                if (result)
                {
                    Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
                }
            });

            return true;
        }

        private void Left_button_Clicked(object sender, EventArgs e)
        {
            if (Index > 0)
            {
                --Index;
                RefreshPage();
            }
        }

        private void Right_button_Clicked(object sender, EventArgs e)
        {
            if (Index < Sql.MaxElement() - 1)
            {
                ++Index;
                RefreshPage();
            }
        }

        private async void Delete_button_Clicked(object sender, EventArgs e)
        {
            if (Sql.MaxElement() == 0)
                return;

            delete_button.IsEnabled = false;

            await Sql.DeleteFromDB(Convert.ToInt32(id.Text));

            if (Index > 0 && Index > Sql.MaxElement() - 1)
                --Index;
            RefreshPage();

            delete_button.IsEnabled = true;
        }

        private async void Manual_add_button_Clicked(object sender, EventArgs e) =>
                await Navigation.PushModalAsync(new ManualAdd());

        public static async Task AddNewEntryWrapper(bool curs, bool pregatire, bool recuperare)
        {
            await Sql.AddNewEntryInDB(curs, pregatire, recuperare);
            Index = Sql.MaxElement() - 1;
        }

        public async Task AddNewEntry(bool curs = false, bool pregatire = false, bool recuperare = false)
        {
            await AddNewEntryWrapper(curs, pregatire, recuperare);
            RefreshPage();
        }

        private void RefreshPage()
        {
            if (Sql.MaxElement() == 0)
            {
                id.Text = date.Text = ora_incepere.Text = ora_sfarsit.Text =
                   curs_alocat.Text = pregatire_alocat.Text = recuperare_alocat.Text =
                   total.Text = "Not found!";

                return;
            }

            (id.Text, date.Text, ora_incepere.Text, ora_sfarsit.Text, curs_alocat.Text, pregatire_alocat.Text, recuperare_alocat.Text, total.Text) =
                (ConversionWrapper((int)Sql.Elements["id"][Index]), ConversionWrapper((DateTime)Sql.Elements["date"][Index]), ConversionWrapper((TimeSpan)Sql.Elements["ora_incepere"][Index]),
                    ConversionWrapper((TimeSpan)Sql.Elements["ora_final"][Index]), ConversionWrapper((TimeSpan)Sql.Elements["curs_alocat"][Index]), ConversionWrapper((TimeSpan)Sql.Elements["pregatire_alocat"][Index]),
                    ConversionWrapper((TimeSpan)Sql.Elements["recuperare_alocat"][Index]), ConversionWrapper((TimeSpan)Sql.Elements["total"][Index]));
        }

        private string ConversionWrapper(object elem)
        {
            switch (elem)
            {
                case int:
                    return elem.ToString();
                case DateTime:
                    return ((DateTime)elem).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                case TimeSpan:
                    return ((TimeSpan)elem).ToString(@"hh\:mm");
                default:
                    throw new ArgumentException();
            }
        }

        private async Task NfcService()
        {
            if (!CrossNFC.IsSupported)
            {
                await DisplayAlert("Error", "NFC is not supported! Please manually add new lessons.", "OK");
                return;
            }

            if (!CrossNFC.Current.IsAvailable)
            {
                await DisplayAlert("Error", "NFC is not available", "OK");
                return;
            }

            if (!CrossNFC.Current.IsEnabled)
            {
                await DisplayAlert("Error", "NFC is disabled", "OK");
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

                await NfcService();
            }
        }

        private void StartListening() =>
                Device.BeginInvokeOnMainThread(() =>
                {
                    SubscribeEventsReal();

                    if (Startup)
                    {
                        indicator.Color = Color.Green;

                        CrossNFC.Current.StartListening();
                        Startup = false;
                    }
                });

        private void StopListening() =>
                Device.BeginInvokeOnMainThread(() =>
                {
                    SubscribeFake();
                });

        private void SubscribeEventsReal()
        {
            if (FakeListener)
                CrossNFC.Current.OnMessageReceived -= Current_OnMessageReceivedFake;
            CrossNFC.Current.OnMessageReceived += Current_OnMessageReceived;
            FakeListener = false;
        }

        private void SubscribeFake()
        {
            CrossNFC.Current.OnMessageReceived -= Current_OnMessageReceived;
            CrossNFC.Current.OnMessageReceived += Current_OnMessageReceivedFake;
            FakeListener = true;
        }

        private async void Current_OnMessageReceived(ITagInfo tagInfo)
        {
            StopListening();

            if (tagInfo == null)
            {
                await DisplayAlert("Error", "No tag found", "OK");
            }
            else if (!tagInfo.IsSupported)
            {
                await DisplayAlert("Error", "Unsupported tag", "OK");
            }
            else if (tagInfo.IsEmpty)
            {
                await DisplayAlert("Error", "Empty tag", "OK");
            }
            else
            {
                _ = FlashColor();

                switch (GetMessage(tagInfo.Records[0]))
                {
                    case "adauga_ora_curs":
                        Ora.curs = true;
                        break;
                    case "adauga_ora_pregatire":
                        Ora.pregatire = true;
                        break;
                    case "adauga_ora_recuperare":
                        Ora.recuperare = true;
                        break;
                    default:
                        break;
                }

                if (!Busy)
                    _ = WaitAndAdd();
            }

            StartListening();
        }

        private async Task WaitAndAdd()
        {
            Busy = true;

            await Countdown();
            await AddNewEntry(Ora.curs, Ora.pregatire, Ora.recuperare);
            (Ora.curs, Ora.pregatire, Ora.recuperare) = (false, false, false);

            Busy = false;
        }

        private async Task Countdown()
        {
            UserDialogs.Instance.ShowLoading("Waiting...");
            await Task.Delay(6000);
            UserDialogs.Instance.HideLoading();
            await Task.Delay(100);
        }

        private async Task FlashColor()
        {
            indicator.Color = Color.Red;
            await Task.Delay(500);
            indicator.Color = Color.Green;
        }

        private void Current_OnMessageReceivedFake(ITagInfo tagInfo)
        {
            // Do nothing;
        }

        private string GetMessage(NFCNdefRecord record)
        {
            if (record.TypeFormat != NFCNdefTypeFormat.WellKnown || (string.IsNullOrWhiteSpace(record.MimeType) && record.MimeType != "text/plain"))
                return null;

            return record.Message;
        }

        private static async Task AlertAndKill(string message)
        {
            await Application.Current.MainPage.DisplayAlert("Error", message, "OK");
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
        }

        public static void ShowAlert(string message) =>
            Device.BeginInvokeOnMainThread(async () =>
                await AlertAndKill(message));

        public static void ShowToast(string message) =>
            Device.BeginInvokeOnMainThread(() =>
                Toast.MakeText(Android.App.Application.Context, message, ToastLength.Short).Show());
    }
}