using System.Threading.Tasks;
using CheckinLS.API.Misc;
using Xamarin.Essentials;

namespace CheckinLS.InterfacesAndClasses.Internet
{
    public class InternetAccess
    {
        public virtual async Task<bool> CheckInternetAsync()
        {
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                return true;

            await HelperFunctions.ShowAlertAsync("No internet connection!", true);
            return false;
        }
    }
}
