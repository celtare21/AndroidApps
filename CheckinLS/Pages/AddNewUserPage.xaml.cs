using System;
using CheckinLS.API.Misc;
using Xamarin.Forms.Xaml;
using MainSql = CheckinLS.API.Sql.MainSql;

// ReSharper disable RedundantCapturedContext

namespace CheckinLS.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddNewUserPage
    {
        private readonly string _password;

        public AddNewUserPage(string password)
        {
            InitializeComponent();

            _password = password;

            Enter.Clicked += Enter_Clicked;
        }

        private async void Enter_Clicked(object sender, EventArgs e)
        {
            if (MainSql.Conn == null)
                return;

            var username = Username.Text;

            if (string.IsNullOrEmpty(username))
                return;

            username = RemoveWhitespace(username.ToLowerInvariant());

            try
            {
                await MainSql.MakeUserAccountAsync(username, _password);
            }
            catch (UserTableNotFound)
            {
                await DisplayAlert("Error", "No table found with that name!", "OK");
                return;
            }
            catch (UserAlreadyExists)
            {
                await DisplayAlert("Error", "User already registered!", "OK");
                return;
            }
            catch (PinAlreadyExists)
            {
                await DisplayAlert("Error", "Password already used!", "OK");
                return;
            }

            await DisplayAlert("New user", "User created! Please re-enter pin", "OK");

            await Navigation.PushModalAsync(new LoginPage());
        }

        private static string RemoveWhitespace(string str) =>
                    string.Join("", str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
    }
}