using System;

namespace CheckinLS.API.Standard
{
    public class StandardDatabaseEntry
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

        // ReSharper disable once UnusedMember.Global
        public StandardDatabaseEntry()
        {
            // Only used for QueryAsync.
        }
    }
}
