using CheckinLS.API.Office;
using CheckinLS.API.Standard;
using CheckinLS.API.Misc;
using CheckinLS.Helpers;
using CheckinLS.InterfacesAndClasses.Users;
using CheckinLS.InterfacesAndClasses.Date;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms.Xaml;

namespace CheckinLS.API.Sql
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public static partial class MainSql
    {
        public static SqlConnection Conn { get; private set; }
        private static string _user;

        public static async Task CreateAsync(IUsers usersInterface, string pin = null)
        {
            await usersInterface.CreateUsersCacheAsync(Conn);

            var accounts = usersInterface.DeserializeCache();

            var result = pin ?? usersInterface.ReadLoggedUser();

            if (result != null && accounts.TryGetValue(result, out _user))
            {
                usersInterface.CreateLoggedUser(pin);
                return;
            }

            var helpers = usersInterface.GetHelpers();
            helpers.DropLoggedAccount();
            helpers.DropCache();

            throw new NoUserFound();
        }

        public static void CreateConnection() =>
                    Conn = new SqlConnection(Secrets.ConnStr);

        public static async Task CkeckConnectionAsync()
        {
            if (Conn?.State == ConnectionState.Closed)
            {
                try
                {
                    await Conn.OpenAsync();
                }
                catch (SqlException)
                {
                    HelperFunctions.ShowAlertKill("Couldn't connect to the database!");
                }
            }

            while (Conn?.State == ConnectionState.Connecting)
                await Task.Delay(100);
        }

        public static Task CloseConnectionAsync() =>
                    Conn?.CloseAsync();

        public static void SetNullConnection() =>
                    Conn = null;

        public static async Task AddToDbAsync(StandardDatabaseEntry entries)
        {
            await CkeckConnectionAsync();

            string query =
                $@"INSERT INTO ""prezenta.{_user}"" VALUES (@date, @oraIncepere, @oraFinal, @cursAlocat, @pregatireAlocat, @recuperareAlocat, @total, @observatii)";

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

        public static async Task AddToDbAsync(OfficeDatabaseEntries entries)
        {
            await CkeckConnectionAsync();

            string query =
                $@"INSERT INTO ""prezenta.office.{_user}"" VALUES (@date, @oraIncepere, @oraFinal, @total)";

            await Conn.ExecuteAsync(query,
                new
                {
                    date = entries.Date,
                    oraIncepere = entries.OraIncepere,
                    oraFinal = entries.OraFinal,
                    total = entries.Total
                }).ConfigureAwait(false);
        }

        public static async Task DeleteFromDbAsync(bool office, int id)
        {
            await CkeckConnectionAsync();

            var query = office
                ? $@"DELETE FROM ""prezenta.office.{_user}"" WHERE id = {id}"
                : $@"DELETE FROM ""prezenta.{_user}"" WHERE id = {id}";


            await Conn.ExecuteAsync(query).ConfigureAwait(false);
        }

        public static async Task DeleteFromDbAsync(bool office, string date)
        {
            await CkeckConnectionAsync();

            var query = office
                ? $@"DELETE FROM ""prezenta.office.{_user}"" WHERE date = '{date}'"
                : $@"DELETE FROM ""prezenta.{_user}"" WHERE date = '{date}'";

            await Conn.ExecuteAsync(query).ConfigureAwait(false);
        }

        public static async Task<IEnumerable<StandardDatabaseEntry>> GetAllElementsStandardAsync()
        {
            await CkeckConnectionAsync();

            try
            {
                return await Conn.QueryAsync<StandardDatabaseEntry>($@"SELECT * FROM ""prezenta.{_user}""");
            }
            catch (SqlException e)
            {
                HelperFunctions.ShowAlertKill(e.Message);
                return null;
            }
        }

        public static async Task<IEnumerable<OfficeDatabaseEntries>> GetAllElementsOfficeAsync()
        {
            await CkeckConnectionAsync();

            try
            {
                return await Conn.QueryAsync<OfficeDatabaseEntries>($@"SELECT * FROM ""prezenta.office.{_user}""");
            }
            catch (SqlException e)
            {
                HelperFunctions.ShowAlertKill(e.Message);
                return null;
            }
        }

        public static async Task<TimeSpan> MaxHourInDbAsync(IGetDate dateInterface)
        {
            await CkeckConnectionAsync();

            var result = await Conn.QueryAsync<TimeSpan?>(
                $@"SELECT oraFinal FROM ""prezenta.{_user}"" WHERE date LIKE '%{dateInterface.GetCurrentDate():yyyy-MM-dd}%'");

            return result.Max() ?? TimeUtils.StartTime();
        }
    }
}
