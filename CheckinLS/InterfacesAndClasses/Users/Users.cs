using Dapper;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms.Xaml;

namespace CheckinLS.InterfacesAndClasses.Users
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public static class Users
    {
        public static Task<string> TryGetUserAsync(SqlConnection conn, string pin) =>
                conn.QuerySingleOrDefaultAsync<string>(@$"SELECT username FROM users WHERE password like '%{pin}%'");

        public static Task<string> ReadLoggedUserAsync() =>
                SecureStorage.GetAsync("localUser");

        public static bool LoggedAccountExists() =>
                string.Equals(Preferences.Get("userCached", "0"), "1");
    }
}