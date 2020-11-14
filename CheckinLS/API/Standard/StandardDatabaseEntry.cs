using System;

namespace CheckinLS.API.Standard
{
    public readonly struct StandardDatabaseEntry
    {
        public readonly int Id;
        public readonly DateTime Date;
        public readonly TimeSpan OraIncepere;
        public readonly TimeSpan OraFinal;
        public readonly TimeSpan CursAlocat;
        public readonly TimeSpan PregatireAlocat;
        public readonly TimeSpan RecuperareAlocat;
        public readonly TimeSpan Total;
        public readonly string Observatii;

        public StandardDatabaseEntry(DateTime date, TimeSpan oraIncepere, TimeSpan oraFinal, TimeSpan cursAlocat,
                                                    TimeSpan pregatireAlocat, TimeSpan recuperareAlocat, TimeSpan total, string observatii) =>
                    (Id, Date, OraIncepere, OraFinal, CursAlocat, PregatireAlocat, RecuperareAlocat, Total, Observatii) =
                            (0, date, oraIncepere, oraFinal, cursAlocat, pregatireAlocat, recuperareAlocat, total, observatii);

        public static bool operator ==(StandardDatabaseEntry x, StandardDatabaseEntry y) =>
            x.Id == y.Id && x.Date == y.Date && x.OraIncepere == y.OraIncepere && x.OraFinal == y.OraFinal &&
            x.CursAlocat == y.CursAlocat && x.PregatireAlocat == y.RecuperareAlocat && x.Total == y.Total &&
            string.Equals(x.Observatii, y.Observatii);

        public static bool operator !=(StandardDatabaseEntry x, StandardDatabaseEntry y) =>
            x.Id != y.Id || x.Date != y.Date || x.OraIncepere != y.OraIncepere ||
            x.OraFinal != y.OraFinal || x.CursAlocat != y.CursAlocat ||
            x.PregatireAlocat != y.RecuperareAlocat || x.Total != y.Total ||
            !string.Equals(x.Observatii, y.Observatii);

        public bool Equals(StandardDatabaseEntry other) =>
                Id == other.Id && Date.Equals(other.Date) && OraIncepere.Equals(other.OraIncepere) &&
                    OraFinal.Equals(other.OraFinal) && CursAlocat.Equals(other.CursAlocat) &&
                    PregatireAlocat.Equals(other.PregatireAlocat) && RecuperareAlocat.Equals(other.RecuperareAlocat) &&
                    Total.Equals(other.Total) && Observatii == other.Observatii;

        public override bool Equals(object obj) =>
            obj is StandardDatabaseEntry other && Equals(other);

        public override int GetHashCode()
        {
            var hashCode = new HashCode();

            hashCode.Add(Id);
            hashCode.Add(Date);
            hashCode.Add(OraIncepere);
            hashCode.Add(OraFinal);
            hashCode.Add(CursAlocat);
            hashCode.Add(PregatireAlocat);
            hashCode.Add(RecuperareAlocat);
            hashCode.Add(Total);
            hashCode.Add(Observatii);

            return hashCode.ToHashCode();
        }
    }
}
