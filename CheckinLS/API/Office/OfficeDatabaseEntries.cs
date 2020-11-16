using System;

namespace CheckinLS.API.Office
{
    public class OfficeDatabaseEntries
    {
        public readonly int Id;
        public readonly DateTime Date;
        public readonly TimeSpan OraIncepere;
        public readonly TimeSpan OraFinal;
        public readonly TimeSpan Total;

        public OfficeDatabaseEntries(DateTime date, TimeSpan oraIncepere, TimeSpan oraFinal, TimeSpan total) =>
            (Id, Date, OraIncepere, OraFinal, Total) = (0, date, oraIncepere, oraFinal, total);

        // ReSharper disable once UnusedMember.Global
        public OfficeDatabaseEntries()
        {
            // Only used for QueryAsync.
        }
    }
}
