using CheckinLS.Helpers;
using CheckinLS.InterfacesAndClasses;
using CheckinLS.Pages;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using Xamarin.Forms.Xaml;
using static CheckinLS.API.SqlUtils;

namespace CheckinLS.API
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainSql
    {
        private static SqlConnection _conn;
        private readonly IGetDate _dateInterface;
        private static string _pin;
        public static string User;
        public List<TableColumns> Elements;

        public static async Task<Tuple<MainSql, int>> CreateAsync(string pin, IGetDate dateInterface)
        {
            if (!MakeConnection())
                return new Tuple<MainSql, int>(null, -1);

            var thisClass = new MainSql(pin, dateInterface);
            var result = await thisClass.HasUserAsync().ConfigureAwait(false);

            if (result == null)
                return new Tuple<MainSql, int>(null, -2);

            User = result;

            await thisClass.RefreshElementsAsync().ConfigureAwait(false);

            return new Tuple<MainSql, int>(thisClass, 0);
        }

        private static bool MakeConnection()
        {
            try
            {
                _conn = new SqlConnection(Secrets.ConnStr);
            }
            catch
            {
                return false;
            }

            return true;
        }

        private MainSql(string pin, IGetDate dateInterface)
        {
            _dateInterface = dateInterface;
            _pin = pin;
        }

        public async Task AddNewEntryInDbAsync(string observatii, bool curs, bool pregatire, bool recuperare)
        {
            await AddToDbAsync(await NewElementsTableAsync(observatii ?? "None", curs, pregatire, recuperare).ConfigureAwait(false))
                .ConfigureAwait(false);

            await RefreshElementsAsync().ConfigureAwait(false);
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

            var date = _dateInterface.GetCurrentDate();

            return new TableColumns(date, oraIncepere, oraFinal, cursAlocat, pregatireAlocat,
                recuperareAlocat, total, observatii);
        }

        private Task AddToDbAsync(TableColumns table)
        {
            string query =
                $@"INSERT INTO ""prezenta.{User}"" VALUES (@date, @oraIncepere, @oraFinal, @cursAlocat, @pregatireAlocat, @recuperareAlocat, @total, @observatii)";

            return _conn.ExecuteAsync(query,
                new
                {
                    date = table.Date,
                    oraIncepere = table.OraIncepere,
                    oraFinal = table.OraFinal,
                    cursAlocat = table.CursAlocat,
                    pregatireAlocat = table.PregatireAlocat,
                    recuperareAlocat = table.RecuperareAlocat,
                    total = table.Total,
                    observatii = table.Observatii
                });
        }

        public async Task DeleteFromDbAsync(int? id = null, string date = null)
        {
            if (!id.HasValue && string.IsNullOrEmpty(date))
                throw new AllParametersFalse();

            if (id.HasValue)
                await _conn.ExecuteAsync($@"DELETE FROM ""prezenta.{User}"" WHERE id = {id}");
            else
                await _conn.ExecuteAsync($@"DELETE FROM ""prezenta.{User}"" WHERE date = '{date}'");

            await RefreshElementsAsync().ConfigureAwait(false);
        }

        private async Task<List<TableColumns>> GetAllElementsAsync()
        {
            await OpenConnectionAsync().ConfigureAwait(false);

            var result = await _conn.QueryAsync<TableColumns>($@"SELECT * FROM ""prezenta.{User}""");

            var elements = result.ToList();

            _conn.Close();

            return elements;
        }

        private async Task<TimeSpan> MaxHourInDbAsync()
        {
            await OpenConnectionAsync().ConfigureAwait(false);

            var result = await _conn.QueryAsync<TimeSpan?>(
                $@"SELECT oraFinal FROM ""prezenta.{User}"" WHERE date LIKE '%{_dateInterface.GetCurrentDate():yyyy-MM-dd}%'");

            var max = result.ToList().Max();

            _conn.Close();

            return max ?? StartTime();
        }

        private static async Task OpenConnectionAsync()
        {
            try
            {
                await _conn.OpenAsync().ConfigureAwait(false);
            }
            catch (SqlException)
            {
                Analytics.TrackEvent("OpenConnectionAsync exception");
                Home.ShowAlertKill("Could not open connection");
            }
        }

        public int MaxElement() =>
                    Elements?.Count ?? 0;
    }
}
