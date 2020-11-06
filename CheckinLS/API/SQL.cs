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
        private readonly bool _isTest;
        public Dictionary<string, List<object>> Elements;

        public static async Task<MainSql> CreateAsync(string user, bool isTest)
        {
            var thisClass = new MainSql(user, isTest);

            if (!await thisClass.IsUserAsync().ConfigureAwait(false))
                return null;

            await thisClass.RefreshElementsAsync().ConfigureAwait(false);

            return thisClass;
        }

        private async Task<bool> IsUserAsync()
        {
            string query = $@"SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'prezenta.{_user}'";
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

        private MainSql(string user, bool isTest)
        {
            const string connStr =
                "//";

            _isTest = isTest;

            CheckInternet();

            try
            {
                _conn = new SqlConnection(connStr);
            }
            catch (SqlException)
            {
                Home.ShowAlertKill("Could not make connection.");
            }

            _user = user;

            if (!_isTest)
                Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
        }

        public async Task AddNewEntryInDbAsync(string observatii, bool curs, bool pregatire, bool recuperare)
        {
            await AddToDbAsync(await NewElementsTableAsync(observatii ?? "None", curs, pregatire, recuperare).ConfigureAwait(false))
                .ConfigureAwait(false);

            await RefreshElementsAsync().ConfigureAwait(false);

            if (!_isTest)
                Home.ShowToast("New entry added!");
        }

        public async Task RefreshElementsAsync() =>
                    Elements = await GetAllElementsAsync().ConfigureAwait(false);

        private async Task<TableColumns> NewElementsTableAsync(string observatii, bool curs, bool pregatire, bool recuperare)
        {
            if (!curs && !pregatire && !recuperare)
            {
                throw new AllParametersFalse();
            }

            (TimeSpan oraIncepere, TimeSpan cursAlocat, TimeSpan pregatireAlocat, TimeSpan recuperareAlocat) =
                (await MaxHourInDbAsync().ConfigureAwait(false), curs ? CursTime() : ZeroTime(), pregatire ? PregatireTime() : ZeroTime(), recuperare ? RecuperareTime() : ZeroTime());

            TimeSpan total = cursAlocat + pregatireAlocat + recuperareAlocat;
            TimeSpan oraFinal = oraIncepere + total;

            if (oraFinal.TotalDays > 1)
            {
                throw new HoursOutOfBounds();
            }

            var date = _isTest? FakeDate() : GetCurrentDate();

            return new TableColumns(date, oraIncepere, oraFinal, cursAlocat, pregatireAlocat,
                recuperareAlocat, total, observatii);
        }

        private async Task AddToDbAsync(TableColumns table)
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

                await ExecuteCommandDbAsync(command).ConfigureAwait(false);
            }
        }

        public async Task DeleteFromDbAsync(int? id = null, string date = null)
        {
            if (!id.HasValue && string.IsNullOrEmpty(date))
                throw new AllParametersFalse();

            string query = id.HasValue ? $@"DELETE FROM ""prezenta.{_user}"" WHERE id = {id}" : $@"DELETE FROM ""prezenta.{_user}"" WHERE date = '{date}'";

            await using (var command = new SqlCommand(query, _conn))
            {
                await ExecuteCommandDbAsync(command).ConfigureAwait(false);
            }

            await RefreshElementsAsync().ConfigureAwait(false);

            if (!_isTest)
                Home.ShowToast("Entry deleted!");
        }

        private async Task<Dictionary<string, List<object>>> GetAllElementsAsync()
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

            await OpenConnectionAsync().ConfigureAwait(false);

            foreach (var elem in columns)
            {
                var query = $@"SELECT {elem} FROM ""prezenta.{_user}""";

                dic.Add(elem, new List<object>());

                await using (var command = new SqlCommand(query, _conn))
                {
                    await using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
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

        private async Task<TimeSpan> MaxHourInDbAsync()
        {
            string query = $@"SELECT ora_final FROM ""prezenta.{_user}"" WHERE date LIKE @SearchTerm";
            var list = new List<TimeSpan?>();

            await OpenConnectionAsync().ConfigureAwait(false);

            await using (var command = new SqlCommand(query, _conn))
            {
                var date = _isTest ? FakeDate() : GetCurrentDate();
                string term = $"%{date}%";

                command.Parameters.AddWithValue("@SearchTerm", term);

                try
                {
                    await using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
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

        internal async Task ExecuteCommandDbAsync(DbCommand command)
        {
            await OpenConnectionAsync().ConfigureAwait(false);

            await command.ExecuteNonQueryAsync().ConfigureAwait(false);

            _conn.Close();
        }

        private async Task OpenConnectionAsync()
        { 
            CheckInternet();

            try
            {
                await _conn.OpenAsync().ConfigureAwait(false);
            }
            catch (SqlException)
            {
                Home.ShowAlertKill("Could not open connection");
            }
        }

        private void CheckInternet()
        {
            if (!_isTest && Connectivity.NetworkAccess != NetworkAccess.Internet)
                Home.ShowAlertKill("No internet connection!");
        }

        private void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            CheckInternet();
        }

        public int MaxElement() =>
                    Elements?["id"].Count ?? 0;
    }
}
