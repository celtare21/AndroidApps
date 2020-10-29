using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using FeedbackLS.Pages;
using Xamarin.Essentials;
using Xamarin.Forms.Xaml;

namespace FeedbackLS.API
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class MainSQL
    {
        private static SqlConnection Conn;
        private string Name;

        public MainSQL(string name)
        {
            const string connStr = "Server=tcp:celtare21.database.windows.net,1433;Initial Catalog=vlad;Persist Security Info=False;User ID=celtare21;Password=Vlad2000.;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

            CheckInternet();

            try
            {
                Conn = new SqlConnection(connStr);
            }
            catch
            {
                Feedback.ShowAlert("Could not make connection.");
            }

            Name = name;

            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
        }

        public async Task AddToDB(string entry1, string entry2, string entry3, string entry4)
        {
            string query = $@"INSERT INTO ""feedback"" (date,name,entry1,entry2,entry3,entry4)" +
                                            "VALUES (@date,@name,@entry1,@entry2,@entry3,@entry4)";

            using (var command = new SqlCommand(query, Conn))
            {
                command.Parameters.AddWithValue("@date", GetCurrentDate());
                command.Parameters.AddWithValue("@name", Name);
                command.Parameters.AddWithValue("@entry1", entry1);
                command.Parameters.AddWithValue("@entry2", entry2);
                command.Parameters.AddWithValue("@entry3", entry3);
                command.Parameters.AddWithValue("@entry4", entry4);

                await ExecuteCommandDB(command).ConfigureAwait(false);
            }

            Feedback.DataAddedKill();
        }

        private string GetCurrentDate() =>
                DateTime.Now.ToString("yyyy-MM-dd");

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
                Feedback.ShowAlert("Could not open connection");
            }
        }

        private static void CheckInternet()
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                Feedback.ShowAlert("No internet connection!");
            }
        }

        private void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            CheckInternet();
        }
    }
}
