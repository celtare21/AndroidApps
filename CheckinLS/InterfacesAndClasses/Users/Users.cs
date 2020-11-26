using CheckinLS.API.Misc;
using Dapper;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms.Xaml;

namespace CheckinLS.InterfacesAndClasses.Users
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class Users : IUsers
    {
        private static readonly string AppFolder = Xamarin.Essentials.FileSystem.AppDataDirectory;
        private static readonly string UsersFolder = Path.Combine(AppFolder, "users");
        private static readonly string JsonPath = Path.Combine(UsersFolder, "accounts.json");

        public async Task CreateUsersCacheAsync(SqlConnection conn)
        {
            if (Directory.Exists(UsersFolder) && File.Exists(JsonPath))
                return;

            var result =
                (await conn.QueryAsync<KeyValuePair<string, string>>(
                    @"SELECT DISTINCT username AS [Value],password AS [Key] FROM users"))
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            Directory.CreateDirectory(UsersFolder);

            await File.WriteAllTextAsync(JsonPath, JsonConvert.SerializeObject(result)).ConfigureAwait(false);
        }

        public Dictionary<string, string> DeserializeCache()
        {
            string jsonString;

            try
            {
                jsonString = File.ReadAllText(JsonPath);
            }
            catch (FileNotFoundException)
            {
                HelperFunctions.ShowAlertKill("Please wipe the app data.");
                return null;
            }

            return JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);
        }

        public static void DropCache()
        {
            File.Delete(JsonPath);
        }
    }
}
