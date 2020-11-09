using CheckinLS.API;
using CheckinLS.InterfacesAndClasses;
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

            (MainSql sqlClass, int returnCode) = await MainSql.CreateAsync(entryPin, new GetDate());

            switch (returnCode)
            {
                case -1:
                    await DisplayAlert("Error", "Couldn't connect to the database!", "OK");
                    App.Close();
                    return;
                case -2:
                    await DisplayAlert("Error", "No user found! Please create one.", "OK");
                    await Navigation.PushModalAsync(new AddUser(entryPin));
                    return;
                default:
                    await Navigation.PushModalAsync(new Home(sqlClass));
                    break;
            }
        }
    }
}