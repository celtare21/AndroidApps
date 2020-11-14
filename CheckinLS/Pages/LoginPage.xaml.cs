using CheckinLS.InterfacesAndClasses.Users;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using MainSql = CheckinLS.API.Sql.MainSql;

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
            if (MainSql.Conn == null)
                return;

            string entryPin = Pin.Text;

            if (string.IsNullOrEmpty(entryPin))
                return;

            Enter.IsEnabled = false;

            (MainSql sqlClass, int returnCode) = await MainSql.CreateAsync(entryPin, new Users());

            switch (returnCode)
            {
                case -1:
                    await DisplayAlert("Error", "No user found! Please create one.", "OK");
                    await Navigation.PushModalAsync(new AddUser(entryPin, sqlClass));
                    return;
                default:
                    var homeClass = new Home();
                    await Navigation.PushModalAsync(homeClass);
                    await homeClass.CreateElementsAsync(sqlClass);
                    homeClass.RefreshPage();
                    await homeClass.NfcServiceAsync();
                    break;
            }
        }
    }
}