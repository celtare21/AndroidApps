using System;
using Xamarin.Forms.Xaml;

namespace CheckinLS.API.Misc
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public static class TimeUtils
    {
        public static TimeSpan StartTime() =>
            TimeSpan.FromHours(10);

        public static TimeSpan CursTime() =>
            TimeSpan.FromHours(1.50);

        public static TimeSpan PregatireTime() =>
            TimeSpan.FromMinutes(30);

        public static TimeSpan RecuperareTime() =>
            TimeSpan.FromMinutes(30);

        public static TimeSpan ZeroTime() =>
            TimeSpan.Zero;
    }
}
