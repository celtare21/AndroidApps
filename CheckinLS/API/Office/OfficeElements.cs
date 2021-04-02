using CheckinLS.API.Misc;
using CheckinLS.InterfacesAndClasses.Date;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task AddNewEntryAsync(DateTime? date, TimeSpan start, TimeSpan finish, string observatii)
        {
            await MainSql.AddToDbAsync(NewElementsTable(date, start, finish, string.IsNullOrEmpty(observatii) ? "None" : observatii));
            await RefreshElementsAsync();
            Index = MaxElement();
        }

        private OfficeDatabaseEntries NewElementsTable(DateTime? date, TimeSpan start, TimeSpan finish, string observatii)
        {
            if (start == finish)
                throw new HoursCantBeEqual();

            if (start > finish)
                throw new StartCantBeBigger();

            TimeSpan total = finish - start;

            if (total.TotalDays > 1)
                throw new HoursOutOfBounds();


            return new OfficeDatabaseEntries
            {
                Date = date ?? _dateInterface.GetCurrentDate(),
                OraIncepere = start,
                OraFinal = finish,
                Total = total,
                Observatii = observatii
            };
        }

        public async Task DeleteEntryAsync(int id)
        {
            await MainSql.DeleteFromDbAsync(true, id, null);
            await RefreshElementsAsync().ConfigureAwait(false);
        }

        private async Task RefreshElementsAsync() =>
            Entries = (await MainSql.GetAllElementsAsync<OfficeDatabaseEntries>()).ToList();

        public int MaxElement() =>
            Entries.Count - 1;
    }
}
