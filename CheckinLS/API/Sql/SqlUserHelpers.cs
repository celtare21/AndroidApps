using CheckinLS.API.Misc;
using Dapper;
using System.Threading.Tasks;

namespace CheckinLS.API.Sql
{
    public static partial class MainSql
    {
        private static async Task<bool> IsUserAlreadyCreatedAsync(string username)
        {
            if (!await HelperFunctions.InternetCheck())
                return false;

            var result = await Conn.QuerySingleOrDefaultAsync<string>($@"SELECT username FROM users WHERE username = '{username}'");

            return string.IsNullOrEmpty(result);
        }

        private static async Task<bool> IsPasswordAlreadyUsedAsync(string password)
        {
            if (!await HelperFunctions.InternetCheck())
                return false;

            var result = await Conn.QuerySingleOrDefaultAsync<string>($@"SELECT username FROM users WHERE password = '{password}'");

            return string.IsNullOrEmpty(result);
        }

        private static async Task<bool> IsUserAsync(string username)
        {
            if (!await HelperFunctions.InternetCheck())
                return false;

            var result = await Conn.QuerySingleOrDefaultAsync<string>(
                $@"SELECT TABLE_CATALOG FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'prezenta.{username}'");

            return string.IsNullOrEmpty(result);
        }

        public static bool UserHasOffice()
        {
            const string office = "alin, vasile, cristi, elena, test";

            return office.Contains(_user);
        }

        public static async Task MakeUserAccountAsync(string username, string password)
        {
            if (await IsUserAsync(username))
                throw new UserTableNotFound();

            if (!await IsUserAlreadyCreatedAsync(username))
                throw new UserAlreadyExists();

            if (!await IsPasswordAlreadyUsedAsync(password))
                throw new PinAlreadyExists();

            const string query = @"INSERT INTO users (username,password)" +
                                 "VALUES (@Username,@Password)";

            if (!await HelperFunctions.InternetCheck())
                return;

            await Conn.ExecuteAsync(query, new {Username = username, Password = password}).ConfigureAwait(false);
        }
    }
}
