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
        public static string ConversionWrapper<T>(T elem)
        {
            return elem switch
            {
                int i => i.ToString(),
                string str => str,
                DateTime time => time.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                TimeSpan span => span.ToString(@"hh\:mm"),
                var _ => throw new ArgumentException()
            };
        }

        public static void ShowAlertKill(string message) =>
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Application.Current.MainPage.DisplayAlert("Error", message, "OK");
                Analytics.TrackEvent(message);
                App.Close();
            });

        public static void ShowToast(string message) =>
            Device.BeginInvokeOnMainThread(() =>
                UserDialogs.Instance.Toast(message));
    }
}
