using Dapper;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms.Xaml;

namespace CheckinLS.InterfacesAndClasses.Users
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class Users : IUsers
    {
        private static readonly string AppFolder = FileSystem.AppDataDirectory;
        private static readonly string UsersFolder = Path.Combine(AppFolder, "users");
        private static readonly string AllAccounts = Path.Combine(UsersFolder, "accounts.json");

        public async Task CreateUsersCacheAsync(SqlConnection conn)
        {
            if (Directory.Exists(UsersFolder) && File.Exists(AllAccounts))
                return;

            var result =
                (await conn.QueryAsync<KeyValuePair<string, string>>(
                    @"SELECT DISTINCT username AS [Value],password AS [Key] FROM users"))
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            Directory.CreateDirectory(UsersFolder);

            await File.WriteAllTextAsync(AllAccounts, JsonConvert.SerializeObject(result)).ConfigureAwait(false);
        }

        public Dictionary<string, string> DeserializeCache()
        {
            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(AllAccounts));
            }
            catch (FileNotFoundException)
            {
                return new Dictionary<string, string>
                {
                    {string.Empty, string.Empty}
                };
            }
        }

        public void CreateLoggedUser(string pin)
        {
            if (!LoggedAccountExists())
                Preferences.Set("localPin", pin);
        }

        public string ReadLoggedUser() =>
                Preferences.Get("localPin", null);

        public static bool LoggedAccountExists() =>
                Preferences.ContainsKey("localPin");

        public UserHelpers GetHelpers() =>
                new UserHelpers();

        public class UserHelpers
        {
            public virtual void DropCache() =>
                File.Delete(AllAccounts);

            public virtual void DropLoggedAccount() =>
                Preferences.Remove("localPin");
        }
    }
}