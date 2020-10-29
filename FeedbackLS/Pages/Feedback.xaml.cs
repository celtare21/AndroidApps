using FeedbackLS.API;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FeedbackLS.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Feedback : ContentPage
    {
        private MainSQL Sql;

        public Feedback(string name)
        {
            InitializeComponent();

            Sql = new MainSQL(name);

            send.Clicked += Send_Clicked;
        }

        protected override bool OnBackButtonPressed()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                var result = await DisplayAlert("Alert!", "Do you really want to exit the application?", "Yes", "No");

                if (result)
                {
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                }
            });

            return true;
        }

        private async void Send_Clicked(object sender, System.EventArgs e)
        {
            send.IsEnabled = false;

            await Sql.AddToDB(entry1.Text ?? string.Empty, entry2.Text ?? string.Empty, entry3.Text ?? string.Empty, entry4.Text ?? string.Empty);
        }

        private static async Task AlertAndKill(string message)
        {
            await Application.Current.MainPage.DisplayAlert("Error", message, "OK");
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        public static void DataAddedKill()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Application.Current.MainPage.DisplayAlert("Done!", "Feedback uploaded!", "Close");
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            });
        }

        public static void ShowAlert(string message) =>
                Device.BeginInvokeOnMainThread(async () =>
                    await AlertAndKill(message));
    }
}