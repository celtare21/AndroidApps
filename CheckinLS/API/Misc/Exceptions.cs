using System;
using Xamarin.Forms.Xaml;

namespace CheckinLS.API.Misc
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class HoursOutOfBounds : Exception
    {
        public HoursOutOfBounds()
        {
            Console.WriteLine(@"Hours out of bounds!");
        }
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class AllParametersFalse : Exception
    {
        public AllParametersFalse()
        {
            Console.WriteLine(@"All parameters are false!");
        }
    }
}
