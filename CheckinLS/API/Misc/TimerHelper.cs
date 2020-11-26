using System;
using System.Timers;

namespace CheckinLS.API.Misc
{
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
