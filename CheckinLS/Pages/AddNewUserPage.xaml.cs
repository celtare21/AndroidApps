﻿using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using MainSql = CheckinLS.API.Sql.MainSql;

// ReSharper disable RedundantCapturedContext

namespace CheckinLS.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddUser
    {
        private readonly string _password;
        private readonly MainSql _sqlClass;

        public AddUser(string password, MainSql sqlClass)
        {
            InitializeComponent();

            _password = password;
            _sqlClass = sqlClass;

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
            var username = Username.Text;

            if (string.IsNullOrEmpty(username))
                return;

            username = RemoveWhitespace(username.ToLowerInvariant());

            var result = await _sqlClass.MakeUserAccountAsync(username, _password);

            switch (result)
            {
                case -1:
                    await DisplayAlert("Error", "No table found with that name!", "OK");
                    return;
                case -2:
                    await DisplayAlert("Error", "User already registered!", "OK");
                    return;
                case -3:
                    await DisplayAlert("Error", "Password already used!", "OK");
                    return;
            }

            await DisplayAlert("New user", "User created! Please re-enter pin", "OK");

            await Navigation.PushModalAsync(new Login());
        }

        private string RemoveWhitespace(string str) =>
                    string.Join("", str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
    }
}