using System;

namespace CheckinLS.API
{
    public class TableColumns
    {
        public int Id { get; }
        public DateTime Date { get; }
        public TimeSpan OraIncepere { get; }
        public TimeSpan OraFinal { get; }
        public TimeSpan CursAlocat { get; }
        public TimeSpan PregatireAlocat { get; }
        public TimeSpan RecuperareAlocat { get; }
        public TimeSpan Total { get; }
        public string Observatii { get; }

        public TableColumns(DateTime date, TimeSpan oraIncepere, TimeSpan oraFinal, TimeSpan cursAlocat,
                                                    TimeSpan pregatireAlocat, TimeSpan recuperareAlocat, TimeSpan total, string observatii) =>
                    (Id, Date, OraIncepere, OraFinal, CursAlocat, PregatireAlocat, RecuperareAlocat, Total, Observatii) =
                            (0, date, oraIncepere, oraFinal, cursAlocat, pregatireAlocat, recuperareAlocat, total, observatii);

        public TableColumns()
        {

        }
    }
}
