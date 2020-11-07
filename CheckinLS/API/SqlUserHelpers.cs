using System.Data.SqlClient;
using System.Threading.Tasks;

namespace CheckinLS.API
{
    public partial class MainSql
    {
        private async Task<string> HasUserAsync()
        {
            string query = $@"SELECT username FROM users WHERE password = '{_pin}'";
            string user = null;

            if (!MakeConnection())
                return null;

            await OpenConnectionAsync().ConfigureAwait(false);

            await using (var command = new SqlCommand(query, _conn))
            {
                await using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    await reader.ReadAsync().ConfigureAwait(false);

                    if (reader.HasRows)
                        user = reader.GetValue(0).ToString();
                }
            }

            _conn.Close();

            return user;
        }

        private static async Task<bool> IsUserAlreadyCreatedAsync(string username)
        {
            string query = $@"SELECT username FROM users WHERE username = '{username}'";
            bool user;

            await OpenConnectionAsync().ConfigureAwait(false);

            await using (var command = new SqlCommand(query, _conn))
            {
                await using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    await reader.ReadAsync().ConfigureAwait(false);

                    user = reader.HasRows;
                }
            }

            _conn.Close();

            return user;
        }

        private static async Task<bool> IsUserAsync(string username)
        {
            string query = $@"SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'prezenta.{username}'";
            bool user;

            await OpenConnectionAsync().ConfigureAwait(false);

            await using (var command = new SqlCommand(query, _conn))
            {
                await using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    await reader.ReadAsync().ConfigureAwait(false);

                    user = reader.HasRows;
                }
            }

            _conn.Close();

            return user;
        }

        public static async Task<int> MakeUserAccountAsync(string username, string password)
        {
            const string query = @"INSERT INTO users (username,password)" +
                                 "VALUES (@username,@password)";

            if (!MakeConnection())
                return -3;

            if (!await IsUserAsync(username))
                return -1;

            if (await IsUserAlreadyCreatedAsync(username))
                return -2;

            await using (var command = new SqlCommand(query, _conn))
            {
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", password);

                await ExecuteCommandDbAsync(command);
            }

            return 0;
        }
    }
}
