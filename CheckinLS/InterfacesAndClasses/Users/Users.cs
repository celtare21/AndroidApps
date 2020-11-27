using CheckinLS.API.Encryption;
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

            Preferences.Clear();
            SecureStorage.RemoveAll();

            var result =
                (await conn.QueryAsync<KeyValuePair<string, string>>(
                    @"SELECT DISTINCT username AS [Value],password AS [Key] FROM users"))
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            Directory.CreateDirectory(UsersFolder);

            await AesKeyHelper.SetAesKeyAsync();

            await File.WriteAllTextAsync(AllAccounts, Aes256Encrypter.Encrypt(JsonConvert.SerializeObject(result), await AesKeyHelper.GetAesKeyAsync())).ConfigureAwait(false);
        }

        public async Task<Dictionary<string, string>> DeserializeCacheAsync()
        {
            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(Aes256Encrypter.Decrypt(await File.ReadAllTextAsync(AllAccounts), await AesKeyHelper.GetAesKeyAsync()));
            }
            catch (FileNotFoundException)
            {
                return new Dictionary<string, string>
                {
                    {string.Empty, string.Empty}
                };
            }
        }

        public async Task CreateLoggedUserAsync(string pin)
        {
            if (!LoggedAccountExists())
            {
                await SecureStorage.SetAsync("localPin", pin);
                Preferences.Set("pinCached", "1");
            }
        }

        public Task<string> ReadLoggedUserAsync() =>
                SecureStorage.GetAsync("localPin");

        public static bool LoggedAccountExists() =>
                string.Equals(Preferences.Get("pinCached", "0"), "1");

        public UserHelpers GetHelpers() =>
                new UserHelpers();

        public class UserHelpers
        {
            public virtual void DropCache() =>
                    File.Delete(AllAccounts);

            public virtual void DropLoggedAccount() =>
                    SecureStorage.Remove("localPin");
        }
    }
}