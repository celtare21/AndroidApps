using CheckinLS.Pages;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms.Xaml;
using static CheckinLS.API.SqlUtils;

namespace CheckinLS.API
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class MainSql
    {
        private static SqlConnection _conn;
        private static string _user;
        public Dictionary<string, List<object>> Elements;

        public static async Task<MainSql> CreateAsync(string user)
        {
            var thisClass = new MainSql(user);

            if (!await IsUser().ConfigureAwait(false))
                return null;

            await thisClass.InitAsync().ConfigureAwait(false);

            return thisClass;
        }

        private static async Task<bool> IsUser()
        {
            string query = $@"SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'prezenta.{_user}'";
            bool user;

            await OpenConnection().ConfigureAwait(false);

            await using (var command = new SqlCommand(query, _conn))
            {
                await using (var reader = await command.ExecuteReaderAsync())
                {
                    await reader.ReadAsync().ConfigureAwait(false);

                    user = reader.HasRows;
                }
            }

            _conn.Close();

            return user;
        }

        private async Task InitAsync()
        {
            await RefreshElements().ConfigureAwait(false);
        }

        private MainSql(string user)
        {
            const string connStr =
                "//";

            CheckInternet();

            try
            {
                _conn = new SqlConnection(connStr);
            }
            catch
            {
                Home.ShowAlertKill("Could not make connection.");
            }

            _user = user;

            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
        }

        public async Task AddNewEntryInDb(string observatii, bool curs, bool pregatire, bool recuperare)
        {
            await AddToDb(await NewElementsTable(observatii ?? "None", curs, pregatire, recuperare).ConfigureAwait(false))
                .ConfigureAwait(false);

            await RefreshElements().ConfigureAwait(false);

            Home.ShowToast("New entry added!");
        }

        public async Task RefreshElements() =>
                    Elements = await GetAllElements().ConfigureAwait(false);

        private async Task<TableColumns> NewElementsTable(string observatii, bool curs, bool pregatire, bool recuperare)
        {
            if (!curs && !pregatire && !recuperare)
            {
                throw new AllParametersFalse();
            }

            (TimeSpan oraIncepere, TimeSpan cursAlocat, TimeSpan pregatireAlocat, TimeSpan recuperareAlocat) =
                (await MaxHourInDb().ConfigureAwait(false), curs ? CursTime() : ZeroTime(), pregatire ? PregatireTime() : ZeroTime(), recuperare ? RecuperareTime() : ZeroTime());

            TimeSpan total = cursAlocat + pregatireAlocat + recuperareAlocat;
            TimeSpan oraFinal = oraIncepere + total;

            if (oraFinal.TotalDays > 1)
            {
                throw new HoursOutOfBounds();
            }

            return new TableColumns(GetCurrentDate(), oraIncepere, oraFinal, cursAlocat, pregatireAlocat,
                recuperareAlocat, total, observatii);
        }

        private async Task AddToDb(TableColumns table)
        {
            string query =
                $@"INSERT INTO ""prezenta.{_user}"" (date,ora_incepere,ora_final,curs_alocat,pregatire_alocat,recuperare_alocat,total,observatii)" +
                                            "VALUES (@date,@ora_incepere,@ora_final,@curs_alocat,@pregatire_alocat,@recuperare_alocat,@total,@observatii)";

            await using (var command = new SqlCommand(query, _conn))
            {
                command.Parameters.AddWithValue("@date", table.Date);
                command.Parameters.AddWithValue("@ora_incepere", table.OraIncepere);
                command.Parameters.AddWithValue("@ora_final", table.OraFinal);
                command.Parameters.AddWithValue("@curs_alocat", table.CursAlocat);
                command.Parameters.AddWithValue("@pregatire_alocat", table.PregatireAlocat);
                command.Parameters.AddWithValue("@recuperare_alocat", table.RecuperareAlocat);
                command.Parameters.AddWithValue("@total", table.Total);
                command.Parameters.AddWithValue("@observatii", table.Observatii);

                await ExecuteCommandDb(command).ConfigureAwait(false);
            }
        }

        public async Task DeleteFromDb(int id)
        {
            string query = $@"DELETE FROM ""prezenta.{_user}"" WHERE id = {id}";

            await using (var command = new SqlCommand(query, _conn))
            {
                await ExecuteCommandDb(command).ConfigureAwait(false);
            }

            await RefreshElements().ConfigureAwait(false);

            Home.ShowToast("Entry deleted!");
        }

        private async Task<Dictionary<string, List<object>>> GetAllElements()
        {
            string[] columns =
            {
                "id",
                "observatii",
                "date",
                "ora_incepere",
                "ora_final",
                "curs_alocat",
                "pregatire_alocat",
                "recuperare_alocat",
                "total"
            };
            var dic = new Dictionary<string, List<object>>();

            await OpenConnection().ConfigureAwait(false);

            foreach (var elem in columns)
            {
                var query = $@"SELECT {elem} FROM ""prezenta.{_user}""";

                dic.Add(elem, new List<object>());

                await using (var command = new SqlCommand(query, _conn))
                {
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            dic[elem].Add(reader.GetValue(0));
                        }
                    }
                }
            }

            _conn.Close();

            return dic;
        }

        private async Task<TimeSpan> MaxHourInDb()
        {
            string query = $@"SELECT ora_final FROM ""prezenta.{_user}"" WHERE date LIKE @SearchTerm";
            var list = new List<TimeSpan?>();

            await OpenConnection().ConfigureAwait(false);

            await using (var command = new SqlCommand(query, _conn))
            {
                string term = $"%{GetCurrentDate()}%";

                command.Parameters.AddWithValue("@SearchTerm", term);

                try
                {
                    await using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            list.Add(reader.GetTimeSpan(0));
                        }
                    }
                }
                catch
                {
                    Home.ShowAlertKill("An error has occured!");
                }
            }

            _conn.Close();

            return list.Max() ?? StartTime();
        }

        private async Task ExecuteCommandDb(DbCommand command)
        {
            await OpenConnection().ConfigureAwait(false);

            await command.ExecuteNonQueryAsync().ConfigureAwait(false);

            _conn.Close();
        }

        private static async Task OpenConnection()
        {
            CheckInternet();

            try
            {
                await _conn.OpenAsync().ConfigureAwait(false);
            }
            catch
            {
                Home.ShowAlertKill("Could not open connection");
            }
        }

        private static void CheckInternet()
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                Home.ShowAlertKill("No internet connection!");
        }

        private void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            CheckInternet();
        }

        public int MaxElement() =>
                        Elements["id"].Count;
    }
}
