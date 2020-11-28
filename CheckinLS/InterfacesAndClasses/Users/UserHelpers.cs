using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms.Xaml;

namespace CheckinLS.InterfacesAndClasses.Users
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class UserHelpers
    {
        public virtual async Task CreateLoggedUserAsync(string user)
        {
            await SecureStorage.SetAsync("localUser", user);
            Preferences.Set("userCached", "1");
        }
    }
}
