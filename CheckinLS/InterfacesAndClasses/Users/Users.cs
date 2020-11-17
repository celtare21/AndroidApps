using CheckinLS.API.Misc;
using CheckinLS.Helpers;
using Dapper;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
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

        public async Task CreateUsersCacheAsync()
        {
            if (Directory.Exists(UsersFolder) && File.Exists(JsonPath))
                return;

            IEnumerable<Accounts> result;

            await using (var conn = new SqlConnection(Secrets.ConnStr))
            {
                result = await conn.QueryAsync<Accounts>(@"SELECT * FROM users");
            }

            Directory.CreateDirectory(UsersFolder);

            await File.WriteAllTextAsync(JsonPath, JsonConvert.SerializeObject(result)).ConfigureAwait(false);
        }

        public List<Accounts> DeserializeCache()
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

            return JsonConvert.DeserializeObject<List<Accounts>>(jsonString);
        }

        public void DropCache()
        {
            File.Delete(JsonPath);
        }
    }
}
