using CheckinLS.API.Misc;
using Xamarin.Essentials;

namespace CheckinLS.InterfacesAndClasses.Internet
{
    public class InternetAccess
    {
        public virtual bool CheckInternet()
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                HelperFunctions.ShowAlertKill("No internet connection!");
                return false;
            }

            return true;
        }
    }
}
