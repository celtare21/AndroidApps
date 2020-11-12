using CheckinLS.Helpers;
using CheckinLS.InterfacesAndClasses;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms.Xaml;

namespace CheckinLS.API
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainSql
    {
        private readonly string _pin;
        private readonly IUsers _usersInterface;
        public static SqlConnection Conn;
        public static string User;

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

            User = result;

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

        public async Task AddToDbAsync(DatabaseEntry table)
        {
            string query =
                $@"INSERT INTO ""prezenta.{User}"" VALUES (@date, @oraIncepere, @oraFinal, @cursAlocat, @pregatireAlocat, @recuperareAlocat, @total, @observatii)";

            await CkeckConnectionAsync();

            await Conn.ExecuteAsync(query,
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
                }).ConfigureAwait(false);
        }

        public async Task DeleteFromDbAsync(int? id = null, string date = null)
        {
            if (!id.HasValue && string.IsNullOrEmpty(date))
                throw new AllParametersFalse();

            await CkeckConnectionAsync();

            string query = id.HasValue
                ? $@"DELETE FROM ""prezenta.{User}"" WHERE id = {id}"
                : $@"DELETE FROM ""prezenta.{User}"" WHERE date = '{date}'";

            await Conn.ExecuteAsync(query).ConfigureAwait(false);
        }

        public async Task<List<DatabaseEntry>> GetAllElementsAsync()
        {
            await CkeckConnectionAsync();

            var result = await Conn.QueryAsync<DatabaseEntry>($@"SELECT * FROM ""prezenta.{User}""");

            var elements = result.ToList();

            return elements;
        }

        public async Task<TimeSpan> MaxHourInDbAsync(IGetDate dateInterface)
        {
            await CkeckConnectionAsync();

            var result = await Conn.QueryAsync<TimeSpan?>(
                $@"SELECT oraFinal FROM ""prezenta.{User}"" WHERE date LIKE '%{dateInterface.GetCurrentDate():yyyy-MM-dd}%'");

            var max = result.ToList().Max();

            return max ?? TimeUtils.StartTime();
        }
    }
}
