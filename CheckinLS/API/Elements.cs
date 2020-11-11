using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CheckinLS.InterfacesAndClasses;
using static CheckinLS.API.TimeUtils;

namespace CheckinLS.API
{
    public class Elements
    {
        public List<DatabaseEntry> Entries;
        public int Index;
        private static MainSql _sql;
        private readonly IGetDate _dateInterface;

        public static async Task<Elements> CreateAsync(MainSql sql, IGetDate dateInterface)
        {
            var elementsClass = new Elements(sql, dateInterface);

            await elementsClass.RefreshElementsAsync();

            return elementsClass;
        }

        private Elements(MainSql sql, IGetDate dateInterface) =>
                (_sql, _dateInterface) = (sql, dateInterface);

        public async Task AddNewEntryAsync(string observatii, bool curs, bool pregatire, bool recuperare)
        {
            await _sql.AddToDbAsync(await NewElementsTableAsync(observatii ?? "None", curs, pregatire, recuperare)).ConfigureAwait(false);
            await RefreshElementsAsync().ConfigureAwait(false);
            Index = MaxElement() - 1;
        }

        public async Task<DatabaseEntry> NewElementsTableAsync(string observatii, bool curs, bool pregatire, bool recuperare)
        {
            if (!curs && !pregatire && !recuperare)
            {
                throw new AllParametersFalse();
            }

            (TimeSpan oraIncepere, TimeSpan cursAlocat, TimeSpan pregatireAlocat, TimeSpan recuperareAlocat) =
                (await _sql.MaxHourInDbAsync(_dateInterface).ConfigureAwait(false), curs ? CursTime() : ZeroTime(), pregatire ? PregatireTime() : ZeroTime(), recuperare ? RecuperareTime() : ZeroTime());

            TimeSpan total = cursAlocat + pregatireAlocat + recuperareAlocat;
            TimeSpan oraFinal = oraIncepere + total;

            if (oraFinal.TotalDays > 1)
            {
                throw new HoursOutOfBounds();
            }

            var date = _dateInterface.GetCurrentDate();

            return new DatabaseEntry(date, oraIncepere, oraFinal, cursAlocat, pregatireAlocat,
                recuperareAlocat, total, observatii);
        }

        public async Task DeleteEntryAsync(int? id = null, string date = null)
        {
            await _sql.DeleteFromDbAsync(id).ConfigureAwait(false);
            await RefreshElementsAsync().ConfigureAwait(false);
        }

        private async Task RefreshElementsAsync() =>
                Entries = await _sql.GetAllElementsAsync().ConfigureAwait(false);

        public int MaxElement() =>
                Entries?.Count ?? 0;
    }
}
