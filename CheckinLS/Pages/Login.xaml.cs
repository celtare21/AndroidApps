using Android.App;
using CheckinLS.API;
using System;
using Xamarin.Forms.Xaml;

#if DEBUG
[assembly: Application(Debuggable = true)]
#else
[assembly: Application(Debuggable = false)]
#endif
namespace CheckinLS.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Login
    {
        public Login()
        {
            InitializeComponent();

            Enter.Clicked += Enter_Clicked;
        }

        private async void Enter_Clicked(object sender, EventArgs e)
        {
            string entryName = Name.Text;

            if (string.IsNullOrEmpty(entryName))
                return;

            Enter.IsEnabled = false;

            var sql = await MainSql.CreateAsync(RemoveWhitespace(entryName).ToLowerInvariant(), false);

            if (sql == null)
            {
                await DisplayAlert("Error", "No user found!", "OK");
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }

            await Navigation.PushModalAsync(new Home(entryName, sql));
        }

        private string RemoveWhitespace(string str) =>
                string.Join("", str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
    }
}