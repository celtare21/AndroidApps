using System;
using Xamarin.Forms.Xaml;

namespace CheckinLS.API
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public static class SqlUtils
    {
        public static string GetCurrentDate() =>
                DateTime.Now.ToString("yyyy-MM-dd");

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
