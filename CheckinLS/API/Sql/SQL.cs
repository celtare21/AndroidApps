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
    public partial class MainSql
    {
        public static SqlConnection Conn { get; private set; }
        private static string _user;
        private readonly string _pin;
        private readonly IUsers _usersInterface;

        public static async Task<Tuple<MainSql, int>> CreateAsync(string pin, IUsers usersInterface)
        {
            var thisClass = new MainSql(pin, usersInterface);

            await thisClass._usersInterface.CreateUsersCacheAsync();

            string result = null;

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var user in thisClass._usersInterface.DeserializeCache())
            {
                if (user.Password == thisClass._pin)
                    result = user.Username;
            }

            if (result == null)
                return new Tuple<MainSql, int>(null, -1);

            _user = result;

            return new Tuple<MainSql, int>(thisClass, 0);
        }

        public static void CreateConnection()
        {
            Conn = new SqlConnection(Secrets.ConnStr);
        }

        private MainSql(string pin, IUsers usersInterface) =>
                (_pin, _usersInterface) = (pin, usersInterface);

        public static async Task CkeckConnectionAsync()
        {
            if (Conn?.State == ConnectionState.Closed)
                await Conn.OpenAsync();
        }

        public static Task CloseConnectionAsync() =>
                    Conn.CloseAsync();

        public static void SetNullConnection() =>
                    Conn = null;

        public static async Task AddToDbAsync(StandardDatabaseEntry standard = default, OfficeDatabaseEntries office = default)
        {
            await CkeckConnectionAsync();

            if (standard == default && office == default)
                return;

            if (standard != default)
            {
                string query =
                    $@"INSERT INTO ""prezenta.{_user}"" VALUES (@date, @oraIncepere, @oraFinal, @cursAlocat, @pregatireAlocat, @recuperareAlocat, @total, @observatii)";

                await Conn.ExecuteAsync(query,
                    new
                    {
                        date = standard.Date,
                        oraIncepere = standard.OraIncepere,
                        oraFinal = standard.OraFinal,
                        cursAlocat = standard.CursAlocat,
                        pregatireAlocat = standard.PregatireAlocat,
                        recuperareAlocat = standard.RecuperareAlocat,
                        total = standard.Total,
                        observatii = standard.Observatii
                    }).ConfigureAwait(false);
            }
            else
            {
                string query = $@"INSERT INTO ""prezenta.office.{_user}"" VALUES (@date, @oraIncepere, @oraFinal, @total)";

                await Conn.ExecuteAsync(query,
                    new
                    {
                        date = office.Date,
                        oraIncepere = office.OraIncepere,
                        oraFinal = office.OraFinal,
                        total = office.Total,
                    }).ConfigureAwait(false);
            }
        }

        public static async Task DeleteFromDbAsync(bool office, int? id = null, string date = null)
        {

            if (!id.HasValue && string.IsNullOrEmpty(date))
                throw new AllParametersFalse();

            await CkeckConnectionAsync();

            string query;

            if (id.HasValue)
            {
                query = office
                    ? $@"DELETE FROM ""prezenta.office.{_user}"" WHERE id = {id}"
                    : $@"DELETE FROM ""prezenta.{_user}"" WHERE id = {id}";
            }
            else
            {
                query = office
                    ? $@"DELETE FROM ""prezenta.office.{_user}"" WHERE date = '{date}'"
                    : $@"DELETE FROM ""prezenta.{_user}"" WHERE date = '{date}'";
            }

            await Conn.ExecuteAsync(query).ConfigureAwait(false);
        }

        public static async Task<List<T>> GetAllElementsAsync<T>()
        {
            await CkeckConnectionAsync();

            IEnumerable<T> result;

            try
            {
                result = typeof(T).Name switch
                {
                    nameof(StandardDatabaseEntry) => await Conn.QueryAsync<T>($@"SELECT * FROM ""prezenta.{_user}"""),
                    nameof(OfficeDatabaseEntries) => await Conn.QueryAsync<T>($@"SELECT * FROM ""prezenta.office.{_user}"""),
                    _ => throw new ArgumentException("Type not implemented")
                };
            }
            catch (SqlException e)
            {
                HelperFunctions.ShowAlertKill(e.Message);
                return null;
            }

            var elements = result.ToList();

            return elements;
        }

        public static async Task<TimeSpan> MaxHourInDbAsync(IGetDate dateInterface)
        {
            await CkeckConnectionAsync();

            var result = await Conn.QueryAsync<TimeSpan?>(
                $@"SELECT oraFinal FROM ""prezenta.{_user}"" WHERE date LIKE '%{dateInterface.GetCurrentDate():yyyy-MM-dd}%'");

            var max = result.ToList().Max();

            return max ?? TimeUtils.StartTime();
        }
    }
}
