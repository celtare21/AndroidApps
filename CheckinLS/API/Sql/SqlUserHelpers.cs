using Dapper;
using System.Linq;
using System.Threading.Tasks;

namespace CheckinLS.API.Sql
{
    public partial class MainSql
    {
        private async Task<bool> IsUserAlreadyCreatedAsync(string username)
        {
            await CkeckConnectionAsync();

            var result = await Conn.QueryAsync<string>($@"SELECT username FROM users WHERE username = '{username}'");

            return result.Any();
        }

        private async Task<bool> IsPasswordAlreadyUsedAsync(string password)
        {
            await CkeckConnectionAsync();

            var result = await Conn.QueryAsync<string>($@"SELECT username FROM users WHERE password = '{password}'");

            return result.Any();
        }

        private async Task<bool> IsUserAsync(string username)
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

        public async Task<int> MakeUserAccountAsync(string username, string password)
        {
            if (!await IsUserAsync(username))
                return -1;

            if (await IsUserAlreadyCreatedAsync(username))
                return -2;

            if (await IsPasswordAlreadyUsedAsync(password))
                return -3;

            const string query = @"INSERT INTO users (username,password)" +
                                 "VALUES (@Username,@Password)";

            await CkeckConnectionAsync();

            await Conn.ExecuteAsync(query, new { Username = username, Password = password });

            return 0;
        }
    }
}
