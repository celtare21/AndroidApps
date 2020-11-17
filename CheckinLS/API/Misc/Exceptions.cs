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

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class HoursCantBeEqual : Exception
    {
        public HoursCantBeEqual()
        {
            Console.WriteLine(@"Bad parameters!");
        }
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class StartCantBeBigger : Exception
    {
        public StartCantBeBigger()
        {
            Console.WriteLine(@"Bad parameters!");
        }
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class NoUserFound : Exception
    {
        public NoUserFound()
        {
            Console.WriteLine(@"No user found!");
        }
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class UserTableNotFound : Exception
    {
        public UserTableNotFound()
        {
            Console.WriteLine(@"No table with that user found!");
        }
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class UserAlreadyExists : Exception
    {
        public UserAlreadyExists()
        {
            Console.WriteLine(@"An accounts with that user already exists!");
        }
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class PinAlreadyExists : Exception
    {
        public PinAlreadyExists()
        {
            Console.WriteLine(@"An accounts with that pin already exists!");
        }
    }
}
