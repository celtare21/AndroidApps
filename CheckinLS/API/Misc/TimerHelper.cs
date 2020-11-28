using System;
using System.Timers;
using Xamarin.Forms.Xaml;

namespace CheckinLS.API.Misc
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public static class TimerHelper
    {
        public static readonly Timer ButtonTimer;
        public static DateTime StartTime;
        public static bool LeftRightButton;
        public const int TimerInternal = 500;

        static TimerHelper() =>
            ButtonTimer = new Timer
            {
                AutoReset = false,
                Enabled = false,
                Interval = TimerInternal
            };
    }
}
