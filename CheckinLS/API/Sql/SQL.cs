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
        private static SqlConnection _conn;
        private static InternetAccess _internetCheck;
        private static string _user;

        public static async Task CreateAsync(UserHelpers usersInterface, InternetAccess internetCheck, string pin = null)
        {
            _internetCheck = internetCheck;

            if (!string.IsNullOrEmpty(pin))
            {
                var result = await Users.TryGetUserAsync(_conn, pin);
                if (string.IsNullOrEmpty(result))
                    throw new NoUserFound();

                await usersInterface.CreateLoggedUserAsync(result);
                _user = result;
            }
            else
            {
                _user = await Users.ReadLoggedUserAsync() ?? throw new UserReadFailed();
            }
        }

        public static void CreateConnection() =>
            _conn = new SqlConnection(Secrets.ConnStr);

        public static bool IsConnNull() =>
            _conn == null;

        public static Task CloseConnectionAsync() =>
            _conn?.CloseAsync();

        public static void SetNullConnection() =>
            _conn = null;

        public static async Task AddToDbAsync<T>(T entry) where T : class
        {
            if (!await HelperFunctions.InternetCheck())
                return;

            var query = entry is StandardDatabaseEntry
                ? $@"INSERT INTO ""prezenta.{_user}"" VALUES (@date, @oraIncepere, @oraFinal, @cursAlocat, @pregatireAlocat, @recuperareAlocat, @total, @observatii)"
                : $@"INSERT INTO ""prezenta.office.{_user}"" VALUES (@date, @oraIncepere, @oraFinal, @total, @observatii)";

            try
            {
                await _conn.ExecuteAsync(query, entry).ConfigureAwait(false);
            }
            catch (SqlException e)
            {
                Analytics.TrackEvent(e.Message);
                await HelperFunctions.ShowAlertAsync("There's been an error processing the data!", true);
            }
        }

        public static async Task DeleteFromDbAsync(bool office, int? id, string date)
        {
            if (!await HelperFunctions.InternetCheck())
                return;

            string query;
            if (id.HasValue)
                query = office
                    ? $@"DELETE FROM ""prezenta.office.{_user}"" WHERE id = {id.Value.ToString()}"
                    : $@"DELETE FROM ""prezenta.{_user}"" WHERE id = {id.Value.ToString()}";
            else
                query = office
                    ? $@"DELETE FROM ""prezenta.office.{_user}"" WHERE date = '{date}'"
                    : $@"DELETE FROM ""prezenta.{_user}"" WHERE date = '{date}'";

            try
            {
                await _conn.ExecuteAsync(query).ConfigureAwait(false);
            }
            catch (SqlException e)
            {
                Analytics.TrackEvent(e.Message);
                await HelperFunctions.ShowAlertAsync("There's been an error processing the data!", true);
            }
        }

        public static async Task<IEnumerable<T>> GetAllElementsAsync<T>() where T : class
        {
            if (!await HelperFunctions.InternetCheck())
                return null;

            try
            {
                return await _conn.QueryAsync<T>(typeof(T) == typeof(OfficeDatabaseEntries)
                    ? $@"SELECT * FROM ""prezenta.office.{_user}"""
                    : $@"SELECT * FROM ""prezenta.{_user}""");
            }
            catch (SqlException e)
            {
                Analytics.TrackEvent(e.Message);
                await HelperFunctions.ShowAlertAsync("Current user isn't registered for office!", false);
                throw;
            }
        }

        public static async Task<TimeSpan> MaxHourInDbAsync(DateTime date)
        {
            if (!await HelperFunctions.InternetCheck())
                return TimeSpan.MinValue;

            IEnumerable<TimeSpan?> result;
            var dateStr = date.ToString("yyyy-MM-dd");
            try
            {
                result = await _conn.QueryAsync<TimeSpan?>(
                    $@"SELECT oraFinal FROM ""prezenta.{_user}"" WHERE date LIKE '%{dateStr}%'");
            }
            catch (SqlException e)
            {
                Analytics.TrackEvent(e.Message);
                await HelperFunctions.ShowAlertAsync("There's been an error processing the data!", true);
                return TimeSpan.MinValue;
            }

            return result.Max() ?? TimeUtils.StartTime();
        }

        public static async Task<bool> CkeckConnectionAsync()
        {
            if (_conn?.State == ConnectionState.Closed)
            {
                try
                {
                    await _conn.OpenAsync();
                }
                catch (SqlException)
                {
                    await HelperFunctions.ShowAlertAsync("Couldn't connect to the database!", true);
                    return false;
                }

                return true;
            }

            while (_conn?.State == ConnectionState.Connecting)
                await Task.Delay(100);

            return await _internetCheck.CheckInternetAsync().ConfigureAwait(false);
        }
    }
}
