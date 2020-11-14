using CheckinLS.API.Misc;
using CheckinLS.InterfacesAndClasses.Date;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms.Xaml;
using MainSql = CheckinLS.API.Sql.MainSql;

namespace CheckinLS.API.Office
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class OfficeElements
    {
        public List<OfficeDatabaseEntries> Entries { get; private set; }
        public int Index;
        private static MainSql _sql;
        private readonly IGetDate _dateInterface;

        public static async Task<OfficeElements> CreateAsync(MainSql sql, IGetDate dateInterface)
        {
            var elementsClass = new OfficeElements(sql, dateInterface);

            await elementsClass.RefreshElementsAsync();

            return elementsClass;
        }

        private OfficeElements(MainSql sql, IGetDate dateInterface) =>
                (_sql, _dateInterface) = (sql, dateInterface);

        public async Task AddNewEntryAsync(TimeSpan start, TimeSpan finish)
        {
            await _sql.AddToDbAsync(office: NewElementsTable(start, finish));
            await RefreshElementsAsync();
            Index = MaxElement() - 1;
        }

        public OfficeDatabaseEntries NewElementsTable(TimeSpan start, TimeSpan finish)
        {
            if (start == finish || start > finish)
            {
                throw new AllParametersFalse();
            }

            TimeSpan total = finish - start;

            if (total.TotalDays > 1)
            {
                throw new HoursOutOfBounds();
            }

            var date = _dateInterface.GetCurrentDate();

            return new OfficeDatabaseEntries(date, start, finish, total);
        }

        public async Task DeleteEntryAsync(int? id = null, string date = null)
        {
            await _sql.DeleteFromDbAsync(true, id);
            await RefreshElementsAsync().ConfigureAwait(false);
        }

        private async Task RefreshElementsAsync() =>
                Entries = await _sql.GetAllElementsAsync<OfficeDatabaseEntries>();

        public int MaxElement() =>
                Entries?.Count ?? 0;
    }
}
