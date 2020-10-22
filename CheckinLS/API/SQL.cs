using CheckinLS.Pages;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms.Xaml;

namespace CheckinLS.API
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class MainSQL
    {
        private static SqlConnection Conn;
        public Dictionary<string, List<object>> Elements;
        private static string User;

        public static async Task<MainSQL> CreateAsync(string user)
        {
            var thisClass = new MainSQL(user);

            if (await IsUser().ConfigureAwait(false) == false)
                return null;

            await thisClass.InitAsync().ConfigureAwait(false);

            return thisClass;
        }

        private static async Task<bool> IsUser()
        {
            bool user = false;
            const string query = "SELECT name FROM sys.Tables";

            await OpenConnection().ConfigureAwait(false);

            using (var command = new SqlCommand(query, Conn))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        string value = reader.GetValue(0).ToString();

                        if (value.Contains($"prezenta.{User}"))
                        {
                            user = true;
                            break;
                        }
                    }
                }
            }

            Conn.Close();

            return user;
        }

        private async Task InitAsync()
        {
            await RefreshElements().ConfigureAwait(false);
        }

        private MainSQL(string user)
        {
            const string connStr = "//";

            CheckInternet();

            try
            {
                Conn = new SqlConnection(connStr);
            }
            catch
            {
                Home.ShowAlert("Could not make connection.");
            }

            User = user;

            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
        }

        public async Task AddNewEntryInDB(bool curs, bool pregatire, bool recuperare)
        {
            await AddToDB(await NewElementsTable(curs, pregatire, recuperare).ConfigureAwait(false)).ConfigureAwait(false);

            await RefreshElements().ConfigureAwait(false);

            Home.ShowToast("New entry added!");
        }

        public async Task RefreshElements() =>
                    Elements = await GetAllElements().ConfigureAwait(false);

        private async Task<TableColumns> NewElementsTable(bool curs, bool pregatire, bool recuperare)
        {
            if (!curs && !pregatire && !recuperare)
            {
                throw new AllParametersFalse();
            }

            (TimeSpan oraIncepere, TimeSpan cursAlocat, TimeSpan pregatireAlocat, TimeSpan recuperareAlocat) =
                (await MaxHourInDB().ConfigureAwait(false), curs ? CursTime() : ZeroTime(), pregatire ? PregatireTime() : ZeroTime(), recuperare ? RecuperareTime() : ZeroTime());

            TimeSpan total = cursAlocat + pregatireAlocat + recuperareAlocat;
            TimeSpan oraFinal = oraIncepere + total;

            if (oraFinal.TotalDays > 1)
            {
                throw new HoursOutOfBounds();
            }

            return new TableColumns(GetCurrentDate(), oraIncepere, oraFinal, cursAlocat, pregatireAlocat, recuperareAlocat, total);
        }

        private async Task AddToDB(TableColumns table)
        {
            string query = $@"INSERT INTO ""prezenta.{User}"" (date,ora_incepere,ora_final,curs_alocat,pregatire_alocat,recuperare_alocat,total)" +
                                            "VALUES (@date,@ora_incepere,@ora_final,@curs_alocat,@pregatire_alocat,@recuperare_alocat,@total)";

            using (var command = new SqlCommand(query, Conn))
            {
                command.Parameters.AddWithValue("@date", table.Date);
                command.Parameters.AddWithValue("@ora_incepere", table.OraIncepere);
                command.Parameters.AddWithValue("@ora_final", table.OraFinal);
                command.Parameters.AddWithValue("@curs_alocat", table.CursAlocat);
                command.Parameters.AddWithValue("@pregatire_alocat", table.PregatireAlocat);
                command.Parameters.AddWithValue("@recuperare_alocat", table.RecuperareAlocat);
                command.Parameters.AddWithValue("@total", table.Total);

                await ExecuteCommandDB(command).ConfigureAwait(false);
            }
        }

        public async Task DeleteFromDB(int id)
        {
            string query = $@"DELETE FROM ""prezenta.{User}"" WHERE id = {id}";

            using (var command = new SqlCommand(query, Conn))
            {
                await ExecuteCommandDB(command).ConfigureAwait(false);
            }

            await RefreshElements().ConfigureAwait(false);

            Home.ShowToast("Entry deleted!");
        }

        private async Task<Dictionary<string, List<object>>> GetAllElements()
        {
            string query;
            string[] columns = { "id", "date", "ora_incepere", "ora_final", "curs_alocat", "pregatire_alocat", "recuperare_alocat", "total" };
            var dic = new Dictionary<string, List<object>>();

            await OpenConnection().ConfigureAwait(false);

            foreach (var elem in columns)
            {
                dic.Add(elem, new List<object>());

                query = $@"SELECT {elem} FROM ""prezenta.{User}""";

                using (var command = new SqlCommand(query, Conn))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            dic[elem].Add(reader.GetValue(0));
                        }
                    }
                }
            }

            Conn.Close();

            return dic;
        }

        private async Task<TimeSpan> MaxHourInDB()
        {
            List<TimeSpan?> list = new List<TimeSpan?>();
            string query = $@"SELECT ora_final FROM ""prezenta.{User}"" WHERE date LIKE @SearchTerm";

            await OpenConnection().ConfigureAwait(false);

            using (var command = new SqlCommand(query, Conn))
            {
                string term = $"%{GetCurrentDate()}%";

                command.Parameters.AddWithValue("@SearchTerm", term);

                using (var reader = command.ExecuteReader())
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        list.Add(reader.GetTimeSpan(0));
                    }
                }
            }

            Conn.Close();

            return list?.Max() ?? StartTime();
        }

        private async Task ExecuteCommandDB(SqlCommand command)
        {
            await OpenConnection().ConfigureAwait(false);

            await command.ExecuteNonQueryAsync().ConfigureAwait(false);

            Conn.Close();
        }

        private static async Task OpenConnection()
        {
            CheckInternet();

            try
            {
                await Conn.OpenAsync().ConfigureAwait(false);
            }
            catch
            {
                Home.ShowAlert("Could not open connection");
            }
        }

        private static void CheckInternet()
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                Home.ShowAlert("No internet connection!");
            }
        }

        private void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            CheckInternet();
        }

        private string GetCurrentDate() =>
                        DateTime.Now.ToString("yyyy-MM-dd");

        private TimeSpan StartTime() =>
                        TimeSpan.FromHours(10);

        private TimeSpan CursTime() =>
                        TimeSpan.FromHours(1.50);

        private TimeSpan PregatireTime() =>
                        TimeSpan.FromMinutes(30);

        private TimeSpan RecuperareTime() =>
                        TimeSpan.FromMinutes(30);

        private TimeSpan ZeroTime() =>
                        TimeSpan.Zero;

        public int MaxElement() =>
                        Elements["id"].Count();
    }

    public readonly struct TableColumns
    {
        public string Date { get; }
        public TimeSpan OraIncepere { get; }
        public TimeSpan OraFinal { get; }
        public TimeSpan CursAlocat { get; }
        public TimeSpan PregatireAlocat { get; }
        public TimeSpan RecuperareAlocat { get; }
        public TimeSpan Total { get; }

        public TableColumns(string date, TimeSpan oraIncepere, TimeSpan oraFinal, TimeSpan cursAlocat, TimeSpan pregatireAlocat, TimeSpan recuperareAlocat, TimeSpan total) =>
                (Date, OraIncepere, OraFinal, CursAlocat, PregatireAlocat, RecuperareAlocat, Total) = (date, oraIncepere, oraFinal, cursAlocat, pregatireAlocat, recuperareAlocat, total);
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class ExecuteFailure : Exception
    {
        public ExecuteFailure(string message)
        {
            Console.WriteLine(message);
        }
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class HoursOutOfBounds : Exception
    {
        public HoursOutOfBounds()
        {
            Console.WriteLine("Hours out of bounds!");
        }
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class AllParametersFalse : Exception
    {
        public AllParametersFalse()
        {
            Console.WriteLine("All parameters are false!");
        }
    }
}
