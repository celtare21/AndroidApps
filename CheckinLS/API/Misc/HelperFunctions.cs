using Acr.UserDialogs;
using Microsoft.AppCenter.Analytics;
using System;
using System.Globalization;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CheckinLS.API.Misc
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public static class HelperFunctions
    {
        public static string ConversionWrapper<T>(T elem) =>
            elem switch
            {
                int i => Convert.ToString(i),
                string str => str,
                DateTime time => time.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                TimeSpan span => span.ToString(@"hh\:mm"),
                var _ => throw new ArgumentException()
            };

        public static void ShowAlertKill(string message) =>
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Application.Current.MainPage.DisplayAlert("Error", message, "OK");
                Analytics.TrackEvent("App crashed");
                App.Close();
            });

        public static void ShowToast(string message) =>
            Device.BeginInvokeOnMainThread(() =>
                UserDialogs.Instance.Toast(message));
    }
}
