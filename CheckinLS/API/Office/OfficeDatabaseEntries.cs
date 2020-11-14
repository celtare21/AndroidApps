using System;

namespace CheckinLS.API.Office
{
    public readonly struct OfficeDatabaseEntries
    {
        public readonly int Id;
        public readonly DateTime Date;
        public readonly TimeSpan OraIncepere;
        public readonly TimeSpan OraFinal;
        public readonly TimeSpan Total;

        public OfficeDatabaseEntries(DateTime date, TimeSpan oraIncepere, TimeSpan oraFinal, TimeSpan total) =>
            (Id, Date, OraIncepere, OraFinal, Total) = (0, date, oraIncepere, oraFinal, total);

        public bool Equals(OfficeDatabaseEntries other) =>
                Id == other.Id && Date.Equals(other.Date) && OraIncepere.Equals(other.OraIncepere) && OraFinal.Equals(other.OraFinal) && Total.Equals(other.Total);

        public override bool Equals(object obj) =>
                obj is OfficeDatabaseEntries other && Equals(other);

        public override int GetHashCode() =>
                HashCode.Combine(Id, Date, OraIncepere, OraFinal, Total);

        public static bool operator ==(OfficeDatabaseEntries x, OfficeDatabaseEntries y) =>
                x.Id == y.Id && x.Date == y.Date && x.OraIncepere == y.OraIncepere && x.Total == y.Total;

        public static bool operator !=(OfficeDatabaseEntries x, OfficeDatabaseEntries y) =>
                x.Id != y.Id || x.Date != y.Date || x.OraIncepere != y.OraIncepere || x.Total != y.Total;
    }
}
