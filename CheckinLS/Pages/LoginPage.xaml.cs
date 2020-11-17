using CheckinLS.InterfacesAndClasses.Users;
using System;
using CheckinLS.API.Misc;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using MainSql = CheckinLS.API.Sql.MainSql;

// ReSharper disable RedundantCapturedContext

namespace CheckinLS.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage
    {
        public LoginPage()
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
            if (MainSql.Conn == null)
                return;

            string entryPin = Pin.Text;

            if (string.IsNullOrEmpty(entryPin))
                return;

            Enter.IsEnabled = false;

            try
            {
                await MainSql.CreateAsync(entryPin, new Users());
            }
            catch (NoUserFound)
            {
                await DisplayAlert("Error", "No user found! Please create one.", "OK");
                await Navigation.PushModalAsync(new AddNewUserPage(entryPin));
                Enter.IsEnabled = true;
                return;
            }

            var homeClass = new Home();
            await Navigation.PushModalAsync(homeClass);
            await homeClass.CreateElementsAsync();
            homeClass.RefreshPage();
            await homeClass.NfcServiceAsync();
        }
    }
}