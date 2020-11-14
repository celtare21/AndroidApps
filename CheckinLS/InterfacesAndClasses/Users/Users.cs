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
            if (Directory.Exists(UsersFolder))
                return;

            IEnumerable<Accounts> result;

            await using (var conn = new SqlConnection(Secrets.ConnStr))
            {
                result = await conn.QueryAsync<Accounts>(@"SELECT * FROM users");
            }

            Directory.CreateDirectory(UsersFolder);
            if (File.Exists(JsonPath))
                File.Delete(JsonPath);

            await File.WriteAllTextAsync(JsonPath, JsonConvert.SerializeObject(result)).ConfigureAwait(false);
        }

        public List<Accounts> DeserializeCache()
        {
            if (!File.Exists(JsonPath))
                return null;

            string jsonString = File.ReadAllText(JsonPath);

            return JsonConvert.DeserializeObject<List<Accounts>>(jsonString);
        }
    }
}
