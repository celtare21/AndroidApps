using CheckinLS.API;
using CheckinLS.Helpers;
using CheckinLS.Pages;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Distribute;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace CheckinLS
{
    public partial class App
    {
        public App()
        {
            InitializeComponent();

            Distribute.UpdateTrack = UpdateTrack.Private;
            AppCenter.Start(Secrets.analytics,
                typeof(Analytics), typeof(Crashes), typeof(Distribute));

            CheckInternet();
            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;

            MainPage = new Login();
        }

        protected override void OnStart()
        {
            Task.Run(MainSql.CreateConnection).ContinueWith(task => MainSql.CkeckConnectionAsync());
        }

        protected override void OnSleep()
        {
            Task.Run(MainSql.CloseConnectionAsync);
        }

        protected override void OnResume()
        {
            Task.Run(MainSql.CkeckConnectionAsync);
        }

        public static void Close()
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        private void CheckInternet()
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                Home.ShowAlertKill("No internet connection!");
        }

        private void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            CheckInternet();
        }
    }
}
