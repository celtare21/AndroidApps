using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FeedbackLS.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class QRScanner : ContentPage
    {
        public QRScanner()
        {
            InitializeComponent();
        }

        private void QRScanner_OnScanResult(ZXing.Result result)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                scanView.IsScanning = false;

                Xamarin.Essentials.Vibration.Vibrate();

                await Navigation.PushModalAsync(new Feedback(result.Text));
            });
        }
    }
}