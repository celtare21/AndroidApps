using Dapper;
using System.Linq;
using System.Threading.Tasks;

namespace CheckinLS.API
{
    public partial class MainSql
    {
        private async Task<string> HasUserAsync()
        {
            if (!MakeConnection())
                return null;

            await OpenConnectionAsync().ConfigureAwait(false);

            var result =
                await _conn.QueryFirstOrDefaultAsync<string>($@"SELECT username FROM users WHERE password = '{_pin}'");

            _conn.Close();

            return result;
        }

        private static async Task<bool> IsUserAlreadyCreatedAsync(string username)
        {
            await OpenConnectionAsync().ConfigureAwait(false);

            var result = await _conn.QueryAsync<string>($@"SELECT username FROM users WHERE username = '{username}'");

            _conn.Close();

            return result.Any();
        }

        private static async Task<bool> IsUserAsync(string username)
        {
            await OpenConnectionAsync().ConfigureAwait(false);

            var result =
                await _conn.QueryAsync<string>(
                    $@"SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'prezenta.{username}'");

            _conn.Close();

            return result.Any();
        }

        public static async Task<int> MakeUserAccountAsync(string username, string password)
        {
            if (!MakeConnection())
                return -3;

            if (!await IsUserAsync(username))
                return -1;

            if (await IsUserAlreadyCreatedAsync(username))
                return -2;

            const string query = @"INSERT INTO users (username,password)" +
                                 "VALUES (@Username,@Password)";

            await _conn.ExecuteAsync(query, new { Username = username, Password = password });

            return 0;
        }
    }
}
