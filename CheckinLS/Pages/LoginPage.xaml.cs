using CheckinLS.InterfacesAndClasses.Users;
using System;
using Acr.UserDialogs;
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

        private async void Enter_Clicked(object sender, EventArgs e)
        {
            if (MainSql.Conn == null)
                return;

            string entryPin = Pin.Text;

            if (string.IsNullOrEmpty(entryPin))
                return;

            UserDialogs.Instance.ShowLoading();

            Enter.IsEnabled = false;

            var usersInterface = new Users();

            try
            {
                await MainSql.CreateAsync(entryPin, usersInterface);
            }
            catch (NoUserFound)
            {
                UserDialogs.Instance.HideLoading();
                await DisplayAlert("Error", "No user found! Please create one.", "OK");
                await Navigation.PushModalAsync(new AddNewUserPage(entryPin));
                usersInterface.DropCache();
                Enter.IsEnabled = true;
                return;
            }

            var homeClass = new Home();
            await Navigation.PushModalAsync(homeClass);
            await homeClass.CreateElementsAsync();
            homeClass.RefreshPage();
            await homeClass.CheckNfcStatusAsync();

            UserDialogs.Instance.HideLoading();
        }

        private void AddEvents() =>
                Enter.Clicked += Enter_Clicked;

        private void RemoveEvents() =>
                Enter.Clicked -= Enter_Clicked;
    }
}