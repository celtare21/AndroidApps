using CheckinLS.API.Sql;
using CheckinLS.InterfacesAndClasses.Internet;
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
        public readonly InternetAccess InternetCheck = new InternetAccess();

        public App()
        {
            InitializeComponent();

            InternetCheck.CheckInternet();

            Distribute.UpdateTrack = UpdateTrack.Private;
            AppCenter.Start(Secrets.analytics,
                typeof(Analytics), typeof(Crashes), typeof(Distribute));

            MainPage = new LoginPage(InternetCheck);
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

        private void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e) =>
            InternetCheck.CheckInternet();

        private void AddEvents() =>
                Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;

        private void RemoveEvents() =>
                Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
    }
}
