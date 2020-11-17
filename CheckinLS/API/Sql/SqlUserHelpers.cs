using Dapper;
using System.Linq;
using System.Threading.Tasks;
using CheckinLS.API.Misc;

namespace CheckinLS.API.Sql
{
    public partial class MainSql
    {
        private static async Task<bool> IsUserAlreadyCreatedAsync(string username)
        {
            await CkeckConnectionAsync();

            var result = await Conn.QueryAsync<string>($@"SELECT username FROM users WHERE username = '{username}'");

            return result.Any();
        }

        private static async Task<bool> IsPasswordAlreadyUsedAsync(string password)
        {
            await CkeckConnectionAsync();

            var result = await Conn.QueryAsync<string>($@"SELECT username FROM users WHERE password = '{password}'");

            return result.Any();
        }

        private static async Task<bool> IsUserAsync(string username)
        {
            await CkeckConnectionAsync();

            var result = await Conn.QueryAsync<string>(
                $@"SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'prezenta.{username}'");

            return result.Any();
        }

        public static bool UserHasOffice()
        {
            var office = new[]
            {
                "alin", "vasile", "test"
            };

            return office.Contains(_user);
        }

        public static async Task MakeUserAccountAsync(string username, string password)
        {
            if (!await IsUserAsync(username))
                throw new UserTableNotFound();

            if (await IsUserAlreadyCreatedAsync(username))
                throw new UserAlreadyExists();

            if (await IsPasswordAlreadyUsedAsync(password))
                throw new PinAlreadyExists();

            const string query = @"INSERT INTO users (username,password)" +
                                 "VALUES (@Username,@Password)";

            await CkeckConnectionAsync();

            await Conn.ExecuteAsync(query, new { Username = username, Password = password }).ConfigureAwait(false);
        }
    }
}
