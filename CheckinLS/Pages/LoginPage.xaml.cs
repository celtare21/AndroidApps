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

            if (!Users.LoggedAccountExists())
                return;

            try
            {
                UserDialogs.Instance.ShowLoading();

                Pin.Text = "0000";
                Enter.IsEnabled = false;

                await MainSql.CreateAsync(new UserHelpers(), _internetCheck);

                if (!await HelperFunctions.InternetCheck())
                    return;

                await CreateHomeClass();

                UserDialogs.Instance.HideLoading();
            }
            catch (UserReadFailed)
            {
                SecureStorage.RemoveAll();
                Preferences.Clear();
                await HelperFunctions.ShowAlertKillAsync("There's been an error, please restart the app!");
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is InvalidOperationException)
            {
                App.Close();
            }
        }

        protected override bool OnBackButtonPressed()
        {
            Device.InvokeOnMainThreadAsync(async () =>
            {
                var result = await DisplayAlert("Alert!", "Do you really want to exit the application?", "Yes", "No");

                if (result)
                    App.Close();
            });

            return true;
        }

        private async void Enter_Clicked(object sender, EventArgs e)
        {
            if (MainSql.IsConnNull())
                return;

            var entryPin = Pin.Text;

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
                if (!await HelperFunctions.InternetCheck())
                    return;

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

            if (!await HelperFunctions.InternetCheck())
                return;

            await CreateHomeClass();

            UserDialogs.Instance.HideLoading();
        }

        private async Task CreateHomeClass()
        {
            var homeClass = new Home();
            await Navigation.PushModalAsync(homeClass);
            await homeClass.CreateElementsAsync();
            homeClass.RefreshPage(true);
        }
    }
}