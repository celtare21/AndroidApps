using CheckinLS.Helpers;
using CheckinLS.Pages;
using System;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Distribute;

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

            MainPage = new Login();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        public static void Close()
        {
            Environment.Exit(0);
        }
    }
}
