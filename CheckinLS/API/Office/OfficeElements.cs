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
        private readonly IGetDate _dateInterface;

        public static async Task<OfficeElements> CreateAsync(IGetDate dateInterface)
        {
            var elementsClass = new OfficeElements(dateInterface);

            await elementsClass.RefreshElementsAsync();

            return elementsClass;
        }

        private OfficeElements(IGetDate dateInterface) =>
                _dateInterface = dateInterface;

        public async Task AddNewEntryAsync(TimeSpan start, TimeSpan finish)
        {
            await MainSql.AddToDbAsync(office: NewElementsTable(start, finish));
            await RefreshElementsAsync();
            Index = MaxElement() - 1;
        }

        private OfficeDatabaseEntries NewElementsTable(TimeSpan start, TimeSpan finish)
        {
            if (start == finish || start > finish)
                throw new AllParametersFalse();

            TimeSpan total = finish - start;

            if (total.TotalDays > 1)
                throw new HoursOutOfBounds();
            
            var date = _dateInterface.GetCurrentDate();

            return new OfficeDatabaseEntries(date, start, finish, total);
        }

        public async Task DeleteEntryAsync(int id)
        {
            await MainSql.DeleteFromDbAsync(true, id);
            await RefreshElementsAsync().ConfigureAwait(false);
        }

        private async Task RefreshElementsAsync() =>
                Entries = await MainSql.GetAllElementsAsync<OfficeDatabaseEntries>();

        public int MaxElement() =>
                Entries?.Count ?? 0;
    }
}
