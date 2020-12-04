using Acr.UserDialogs;
using CheckinLS.API.Misc;
using CheckinLS.InterfacesAndClasses.Users;
using System;
using System.Data;
using System.Threading.Tasks;
using CheckinLS.InterfacesAndClasses.Internet;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using MainSql = CheckinLS.API.Sql.MainSql;

namespace CheckinLS.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage
    {
        private readonly InternetAccess _internetCheck;

        public LoginPage(InternetAccess internetCheck)
        {
            InitializeComponent();

            _internetCheck = internetCheck;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (Users.LoggedAccountExists())
            {
                try
                {
                    UserDialogs.Instance.ShowLoading();

                    Pin.Text = "0000";
                    Enter.IsEnabled = false;

                    await MainSql.CreateAsync(new UserHelpers(), _internetCheck);

                    if (!await MainSql.CkeckConnectionAsync())
                    {
                        HelperFunctions.ShowAlertKill("No internet connection!");
                        return;
                    }

                    var homeClass = new Home();
                    await Navigation.PushModalAsync(homeClass);
                    await homeClass.CreateElementsAsync();
                    homeClass.RefreshPage();
                    await homeClass.CheckNfcStatusAsync();

                    UserDialogs.Instance.HideLoading();
                }
                catch (UserReadFailed)
                {
                    SecureStorage.RemoveAll();
                    Preferences.Clear();
                    HelperFunctions.ShowAlertKill("There's been an error, please restart the app!");
                }
                catch (Exception ex) when (ex is TaskCanceledException || ex is InvalidOperationException)
                {
                    App.Close();
                }
            }
            else
            {
                AddEvents();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (!Users.LoggedAccountExists())
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

            try
            {
                await MainSql.CreateAsync(new UserHelpers(), _internetCheck, entryPin);
            }
            catch (NoUserFound)
            {
                if (!await MainSql.CkeckConnectionAsync())
                {
                    HelperFunctions.ShowAlertKill("No internet connection!");
                    return;
                }
                UserDialogs.Instance.HideLoading();
                await DisplayAlert("Error", "No user found! Please create one.", "OK");
                await Navigation.PushModalAsync(new AddNewUserPage(entryPin));
                Enter.IsEnabled = true;
                return;
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is DataException)
            {
                App.Close();
            }

            if (!await MainSql.CkeckConnectionAsync())
            {
                HelperFunctions.ShowAlertKill("No internet connection!");
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