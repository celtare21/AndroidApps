using CheckinLS.API;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
// ReSharper disable RedundantCapturedContext

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

        private async void Enter_Clicked(object sender, EventArgs e)
        {
            string entryPin = Pin.Text;

            if (string.IsNullOrEmpty(entryPin))
                return;

            Enter.IsEnabled = false;

            var sql = await MainSql.CreateAsync(entryPin, false);

            if (sql == null)
            {
                await DisplayAlert("Error", "No user found! Please create one.", "OK");
                await Navigation.PushModalAsync(new AddUser(entryPin));
                return;
            }

            await Navigation.PushModalAsync(new Home(sql));
        }
    }
}