using CheckinLS.API.Office;
using CheckinLS.API.Standard;
using CheckinLS.API.Misc;
using CheckinLS.Helpers;
using CheckinLS.InterfacesAndClasses.Users;
using CheckinLS.InterfacesAndClasses.Internet;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using Xamarin.Forms.Xaml;

namespace CheckinLS.API.Sql
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public static partial class MainSql
    {
        public static SqlConnection Conn { get; private set; }
        private static InternetAccess _internetCheck;
        private static string _user;

        public static async Task CreateAsync(UserHelpers usersInterface, InternetAccess internetCheck, string pin = null)
        {
            _internetCheck = internetCheck;

            if (!string.IsNullOrEmpty(pin))
            {
                var result = await Users.TryGetUserAsync(Conn, pin);
                if (string.IsNullOrEmpty(result))
                    throw new NoUserFound();

                await usersInterface.CreateLoggedUserAsync(result);
                _user = result;
                return;
            }

            _user = await Users.ReadLoggedUserAsync() ?? throw new UserReadFailed();
        }

        public static void CreateConnection() =>
                    Conn = new SqlConnection(Secrets.ConnStr);

        public static async Task<bool> CkeckConnectionAsync()
        {
            if (Conn?.State == ConnectionState.Closed)
            {
                try
                {
                    await Conn.OpenAsync();
                    return true;
                }
                catch (SqlException)
                {
                    await HelperFunctions.ShowAlertKillAsync("Couldn't connect to the database!");
                    return false;
                }
            }

            while (Conn?.State == ConnectionState.Connecting)
                await Task.Delay(100);

            return await _internetCheck.CheckInternetAsync().ConfigureAwait(false);
        }

        public static Task CloseConnectionAsync() =>
                    Conn?.CloseAsync();

        public static void SetNullConnection() =>
                    Conn = null;

        public static async Task AddToDbAsync(StandardDatabaseEntry entries)
        {
            if (!await CkeckConnectionAsync())
            {
                await HelperFunctions.ShowAlertKillAsync("No internet connection!");
                return;
            }

            var query =
                $@"INSERT INTO ""prezenta.{_user}"" VALUES (@date, @oraIncepere, @oraFinal, @cursAlocat, @pregatireAlocat, @recuperareAlocat, @total, @observatii)";

            try
            {
                await Conn.ExecuteAsync(query,
                    new
                    {
                        date = entries.Date,
                        oraIncepere = entries.OraIncepere,
                        oraFinal = entries.OraFinal,
                        cursAlocat = entries.CursAlocat,
                        pregatireAlocat = entries.PregatireAlocat,
                        recuperareAlocat = entries.RecuperareAlocat,
                        total = entries.Total,
                        observatii = entries.Observatii
                    }).ConfigureAwait(false);
            }
            catch (SqlException e)
            {
                await HelperFunctions.ShowAlertKillAsync("There's been an error processing the data!");
                Analytics.TrackEvent(e.Message);
            }
        }

        public static async Task AddToDbAsync(OfficeDatabaseEntries entries)
        {
            if (!await CkeckConnectionAsync())
            {
                await HelperFunctions.ShowAlertKillAsync("No internet connection!");
                return;
            }

            var query =
                $@"INSERT INTO ""prezenta.office.{_user}"" VALUES (@date, @oraIncepere, @oraFinal, @total, @observatii)";

            try
            {
                await Conn.ExecuteAsync(query,
                    new
                    {
                        date = entries.Date,
                        oraIncepere = entries.OraIncepere,
                        oraFinal = entries.OraFinal,
                        total = entries.Total,
                        observatii = entries.Observatii
                    }).ConfigureAwait(false);
            }
            catch (SqlException e)
            {
                await HelperFunctions.ShowAlertKillAsync("There's been an error processing the data!");
                Analytics.TrackEvent(e.Message);
            }
        }

        public static async Task DeleteFromDbAsync(bool office, int id)
        {
            if (!await CkeckConnectionAsync())
            {
                await HelperFunctions.ShowAlertKillAsync("No internet connection!");
                return;
            }

            var query = office
                ? $@"DELETE FROM ""prezenta.office.{_user}"" WHERE id = {id.ToString()}"
                : $@"DELETE FROM ""prezenta.{_user}"" WHERE id = {id.ToString()}";

            try
            {
                await Conn.ExecuteAsync(query).ConfigureAwait(false);
            }
            catch (SqlException e)
            {
                await HelperFunctions.ShowAlertKillAsync("There's been an error processing the data!");
                Analytics.TrackEvent(e.Message);
            }
        }

        public static async Task DeleteFromDbAsync(bool office, string date)
        {
            if (!await CkeckConnectionAsync())
            {
                await HelperFunctions.ShowAlertKillAsync("No internet connection!");
                return;
            }

            var query = office
                ? $@"DELETE FROM ""prezenta.office.{_user}"" WHERE date = '{date}'"
                : $@"DELETE FROM ""prezenta.{_user}"" WHERE date = '{date}'";

            try
            {
                await Conn.ExecuteAsync(query).ConfigureAwait(false);
            }
            catch (SqlException e)
            {
                await HelperFunctions.ShowAlertKillAsync("There's been an error processing the data!");
                Analytics.TrackEvent(e.Message);
            }
        }

        public static async Task<IEnumerable<StandardDatabaseEntry>> GetAllElementsStandardAsync()
        {
            if (!await CkeckConnectionAsync())
            {
                await HelperFunctions.ShowAlertKillAsync("No internet connection!");
                return null;
            }

            try
            {
                return await Conn.QueryAsync<StandardDatabaseEntry>($@"SELECT * FROM ""prezenta.{_user}""");
            }
            catch (SqlException e)
            {
                await HelperFunctions.ShowAlertKillAsync("There's been an error processing the data!");
                Analytics.TrackEvent(e.Message);
                return null;
            }
        }

        public static async Task<IEnumerable<OfficeDatabaseEntries>> GetAllElementsOfficeAsync()
        {
            if (!await CkeckConnectionAsync())
            {
                await HelperFunctions.ShowAlertKillAsync("No internet connection!");
                return null;
            }

            try
            {
                return await Conn.QueryAsync<OfficeDatabaseEntries>($@"SELECT * FROM ""prezenta.office.{_user}""");
            }
            catch (SqlException e)
            {
                await HelperFunctions.ShowAlertKillAsync("There's been an error processing the data!");
                Analytics.TrackEvent(e.Message);
                return null;
            }
        }

        public static async Task<TimeSpan> MaxHourInDbAsync(DateTime date)
        {
            if (!await CkeckConnectionAsync())
            {
                await HelperFunctions.ShowAlertKillAsync("No internet connection!");
                return TimeSpan.MinValue;
            }

            IEnumerable<TimeSpan?> result;
            var dateStr = date.ToString("yyyy-MM-dd");
            try
            {
                result = await Conn.QueryAsync<TimeSpan?>(
                    $@"SELECT oraFinal FROM ""prezenta.{_user}"" WHERE date LIKE '%{dateStr}%'");
            }
            catch (SqlException e)
            {
                await HelperFunctions.ShowAlertKillAsync("There's been an error processing the data!");
                Analytics.TrackEvent(e.Message);
                return TimeSpan.MinValue;
            }

            return result.Max() ?? TimeUtils.StartTime();
        }
    }
}
