using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using System.Linq;
using System.Threading.Tasks;
using CheckinLS.Helpers;

namespace CheckinLS.API
{
    public partial class MainSql
    {
        private static async Task<bool> IsUserAlreadyCreatedAsync(string username)
        {
            IEnumerable<string> result;

            await using (var conn = new SqlConnection(Secrets.ConnStr))
            {
                result =
                    await conn.QueryAsync<string>($@"SELECT username FROM users WHERE username = '{username}'");
            }

            return result.Any();
        }

        private static async Task<bool> IsPasswordAlreadyUsedAsync(string password)
        {
            IEnumerable<string> result;

            await using (var conn = new SqlConnection(Secrets.ConnStr))
            {
                result =
                    await conn.QueryAsync<string>($@"SELECT username FROM users WHERE password = '{password}'");
            }

            return result.Any();
        }

        private static async Task<bool> IsUserAsync(string username)
        {
            IEnumerable<string> result;

            await using (var conn = new SqlConnection(Secrets.ConnStr))
            {
                result =
                    await conn.QueryAsync<string>(
                        $@"SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'prezenta.{username}'");
            }

            return result.Any();
        }

        public static async Task<int> MakeUserAccountAsync(string username, string password)
        {
            if (!await IsUserAsync(username))
                return -1;

            if (await IsUserAlreadyCreatedAsync(username))
                return -2;

            if (await IsPasswordAlreadyUsedAsync(password))
                return -3;

            const string query = @"INSERT INTO users (username,password)" +
                                 "VALUES (@Username,@Password)";

            await using (var conn = new SqlConnection(Secrets.ConnStr))
            {
                await conn.ExecuteAsync(query, new {Username = username, Password = password});
            }

            return 0;
        }
    }
}
