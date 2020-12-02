using System;
using Xamarin.Forms.Xaml;

namespace CheckinLS.API.Office
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class OfficeDatabaseEntries
    {
        public readonly int Id;
        public readonly DateTime Date;
        public readonly TimeSpan OraIncepere;
        public readonly TimeSpan OraFinal;
        public readonly TimeSpan Total;
        public readonly string Observatii;

        public OfficeDatabaseEntries(DateTime date, TimeSpan oraIncepere, TimeSpan oraFinal, TimeSpan total, string observatii) =>
            (Id, Date, OraIncepere, OraFinal, Total, Observatii) = (0, date, oraIncepere, oraFinal, total, observatii);

        // ReSharper disable once UnusedMember.Global
        public OfficeDatabaseEntries()
        {
            // Only used for QueryAsync.
        }
    }
}
