using CheckinLS.Helpers;
using CheckinLS.InterfacesAndClasses;
using Dapper;
using System;
using System.Collections.Generic;
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

        private MainSql(string pin, IUsers usersInterface) =>
                (_pin, _usersInterface) = (pin, usersInterface);

        public async Task AddToDbAsync(DatabaseEntry table)
        {
            string query =
                $@"INSERT INTO ""prezenta.{User}"" VALUES (@date, @oraIncepere, @oraFinal, @cursAlocat, @pregatireAlocat, @recuperareAlocat, @total, @observatii)";

            await using (var conn = new SqlConnection(Secrets.ConnStr))
            {
                await conn.ExecuteAsync(query,
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
        }

        public async Task DeleteFromDbAsync(int? id = null, string date = null)
        {
            if (!id.HasValue && string.IsNullOrEmpty(date))
                throw new AllParametersFalse();

            await using (var conn = new SqlConnection(Secrets.ConnStr))
            {
                if (id.HasValue)
                    await conn.ExecuteAsync($@"DELETE FROM ""prezenta.{User}"" WHERE id = {id}");
                else
                    await conn.ExecuteAsync($@"DELETE FROM ""prezenta.{User}"" WHERE date = '{date}'");
            }
        }

        public async Task<List<DatabaseEntry>> GetAllElementsAsync()
        {
            IEnumerable<DatabaseEntry> result;

            await using (var conn = new SqlConnection(Secrets.ConnStr))
            {
                result = await conn.QueryAsync<DatabaseEntry>($@"SELECT * FROM ""prezenta.{User}""");
            }

            var elements = result.ToList();

            return elements;
        }

        public async Task<TimeSpan> MaxHourInDbAsync(IGetDate dateInterface)
        {
            IEnumerable<TimeSpan?> result;

            await using (var conn = new SqlConnection(Secrets.ConnStr))
            {
                result = await conn.QueryAsync<TimeSpan?>(
                    $@"SELECT oraFinal FROM ""prezenta.{User}"" WHERE date LIKE '%{dateInterface.GetCurrentDate():yyyy-MM-dd}%'");
            }

            var max = result.ToList().Max();

            return max ?? TimeUtils.StartTime();
        }
    }
}
