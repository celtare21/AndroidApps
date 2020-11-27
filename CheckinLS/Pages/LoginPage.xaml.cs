using Acr.UserDialogs;
using CheckinLS.API.Misc;
using CheckinLS.InterfacesAndClasses.Users;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using MainSql = CheckinLS.API.Sql.MainSql;

namespace CheckinLS.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage
    {
        private readonly IUsers _usersInterface;

        public LoginPage()
        {
            InitializeComponent();

            _usersInterface = new Users();
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

                    await MainSql.CreateAsync(_usersInterface);

                    await MainSql.CkeckConnectionAsync();

                    var homeClass = new Home();
                    await Navigation.PushModalAsync(homeClass);
                    await homeClass.CreateElementsAsync();
                    homeClass.RefreshPage();
                    await homeClass.CheckNfcStatusAsync();

                    UserDialogs.Instance.HideLoading();
                }
                catch (TaskCanceledException)
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
                await MainSql.CreateAsync(_usersInterface, entryPin);
            }
            catch (NoUserFound)
            {
                await MainSql.CkeckConnectionAsync();
                UserDialogs.Instance.HideLoading();
                await DisplayAlert("Error", "No user found! Please create one.", "OK");
                await Navigation.PushModalAsync(new AddNewUserPage(entryPin));
                Enter.IsEnabled = true;
                return;
            }

            await MainSql.CkeckConnectionAsync();

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