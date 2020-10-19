using Android.App;
using CheckinLS.API;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

#if DEBUG
[assembly: Application(Debuggable = true)]
#else
[assembly: Application(Debuggable = false)]
#endif
namespace CheckinLS.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Login : ContentPage
    {
        public Login()
        {
            InitializeComponent();

            enter.Clicked += Enter_Clicked;
        }

        private async void Enter_Clicked(object sender, EventArgs e)
        {
            string entryName = name.Text;

            if (string.IsNullOrEmpty(entryName))
                return;

            enter.IsEnabled = false;

            var sql = await MainSQL.CreateAsync(entryName);

            if (sql == null)
            {
                await DisplayAlert("Error", "No user found!", "OK");
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }

            var homePage = new Home(entryName, sql);
            await Navigation.PushModalAsync(homePage);
        }
    }
}