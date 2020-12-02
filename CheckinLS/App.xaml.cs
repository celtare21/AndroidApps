using CheckinLS.API.Misc;
using CheckinLS.API.Sql;
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

            CheckInternet();

            Distribute.UpdateTrack = UpdateTrack.Private;
            AppCenter.Start(Secrets.analytics,
                typeof(Analytics), typeof(Crashes), typeof(Distribute));

            MainPage = new LoginPage();
        }

        protected override void OnStart()
        {
            AddEvents();
            Task.Run(MainSql.CreateConnection);
        }

        protected override void OnSleep()
        {
            RemoveEvents();
            Task.Run(MainSql.CloseConnectionAsync).ConfigureAwait(false);
        }

        protected override void OnResume()
        {
            AddEvents();
            Task.Run(MainSql.CkeckConnectionAsync).ConfigureAwait(false);
        }

        public static void Close() =>
                System.Diagnostics.Process.GetCurrentProcess().Kill();

        private static void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e) =>
                CheckInternet();

        public static bool CheckInternet()
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                HelperFunctions.ShowAlertKill("No internet connection!");
                return false;
            }

            return true;
        }

        private static void AddEvents() =>
                Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;

        private static void RemoveEvents() =>
                Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
    }
}
