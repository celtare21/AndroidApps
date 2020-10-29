using System;
using Xamarin.Forms.Xaml;

namespace CheckinLS.API
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class ExecuteFailure : Exception
    {
        public ExecuteFailure(string message)
        {
            Console.WriteLine(message);
        }
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class HoursOutOfBounds : Exception
    {
        public HoursOutOfBounds()
        {
            Console.WriteLine("Hours out of bounds!");
        }
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class AllParametersFalse : Exception
    {
        public AllParametersFalse()
        {
            Console.WriteLine("All parameters are false!");
        }
    }
}
