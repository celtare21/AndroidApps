using Android.Widget;
using CheckinLS.API;
using System;
using System.Globalization;
using System.Threading.Tasks;
using Plugin.NFC;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CheckinLS.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Home : ContentPage
    {
        private static MainSQL Sql;
        public static string Name;
        private bool EmptyDB, FakeListener, Startup = true;
        private int Index = 0;

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
            if (EmptyDB)
                return;

            delete_button.IsEnabled = false;

            await Sql.DeleteFromDB(Convert.ToInt32(id.Text));

            if (Index > 0 && Index > Sql.MaxElement() - 1)
                --Index;
            RefreshPage();

            delete_button.IsEnabled = true;
        }

        private async void Manual_add_button_Clicked(object sender, EventArgs e)
        {
            var page = new ManualAdd();

            await Navigation.PushModalAsync(page);
        }

        public static async Task AddNewEntryWrapper(bool pregatire, bool curs, bool recuperare) =>
                await Sql.AddNewEntryInDB(pregatire, curs, recuperare);

        private async Task AddNewEntry(bool pregatire = false, bool curs = false, bool recuperare = false)
        {
            await AddNewEntryWrapper(pregatire, curs, recuperare);
            Index = Sql.MaxElement() - 1;
            RefreshPage();
        }

        private void RefreshPage()
        {
            if (Sql.MaxElement() == 0)
            {
                id.Text = date.Text = ora_incepere.Text = ora_sfarsit.Text =
                   curs_alocat.Text = pregatire_alocat.Text = recuperare_alocat.Text =
                   total.Text = "Not found!";

                EmptyDB = true;

                return;
            }
            else
            {
                EmptyDB = false;
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
                return;
            }

            StartListening();
        }

        private void StartListening() =>
                Device.BeginInvokeOnMainThread(() =>
                {
                    indicator.Color = Color.Green;

                    SubscribeEventsReal();

                    if (Startup)
                    {
                        CrossNFC.Current.StartListening();
                        Startup = false;
                    }
                });

        private void StopListening() =>
                Device.BeginInvokeOnMainThread(() =>
                {
                    indicator.Color = Color.Red;

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
                return;
            }

            if (!tagInfo.IsSupported)
            {
                await DisplayAlert("Error", "Unsupported tag", "OK");
            }
            else if (tagInfo.IsEmpty)
            {
                await DisplayAlert("Error", "Empty tag", "OK");
            }
            else
            {
                if (GetMessage(tagInfo.Records[0]) == "adauga_ora_standard")
                {
                    await AddNewEntry(pregatire: true, curs: true);
                } else if (GetMessage(tagInfo.Records[0]) == "adauga_ora_recuperare")
                {
                    await AddNewEntry(recuperare: true);
                }
            }

            StartListening();
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
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        public static void ShowAlert(string message) =>
            Device.BeginInvokeOnMainThread(async () =>
                await AlertAndKill(message));

        public static void ShowToast(string message) =>
            Device.BeginInvokeOnMainThread(() =>
                Toast.MakeText(Android.App.Application.Context, message, ToastLength.Short).Show());
    }
}