using CheckinLS.API.Misc;
using CheckinLS.InterfacesAndClasses.Date;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms.Xaml;
using static CheckinLS.API.Misc.TimeUtils;
using MainSql = CheckinLS.API.Sql.MainSql;

namespace CheckinLS.API.Standard
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class StandardElements
    {
        public List<StandardDatabaseEntry> Entries { get; private set; }
        public int Index;
        private readonly IGetDate _dateInterface;

        public static async Task<StandardElements> CreateAsync(IGetDate dateInterface)
        {
            var elementsClass = new StandardElements(dateInterface);

            await elementsClass.RefreshElementsAsync();

            return elementsClass;
        }

        private StandardElements(IGetDate dateInterface) =>
                (_dateInterface) = (dateInterface);

        public async Task AddNewEntryAsync(string observatii, bool curs, bool pregatire, bool recuperare)
        {
            await MainSql.AddToDbAsync(await NewElementsTableAsync(string.IsNullOrEmpty(observatii) ? "None" : observatii, curs, pregatire, recuperare));
            await RefreshElementsAsync();
            Index = MaxElement() - 1;
        }

        private async Task<StandardDatabaseEntry> NewElementsTableAsync(string observatii, bool curs, bool pregatire, bool recuperare)
        {
            if (!curs && !pregatire && !recuperare)
            {
                throw new AllParametersFalse();
            }

            (TimeSpan oraIncepere, TimeSpan cursAlocat, TimeSpan pregatireAlocat, TimeSpan recuperareAlocat) =
                (await MainSql.MaxHourInDbAsync(_dateInterface), curs ? CursTime() : ZeroTime(), pregatire ? PregatireTime() : ZeroTime(), recuperare ? RecuperareTime() : ZeroTime());

            TimeSpan total = cursAlocat + pregatireAlocat + recuperareAlocat;
            TimeSpan oraFinal = oraIncepere + total;

            if (oraFinal.TotalDays > 1)
            {
                throw new HoursOutOfBounds();
            }

            var date = _dateInterface.GetCurrentDate();

            return new StandardDatabaseEntry(date, oraIncepere, oraFinal, cursAlocat, pregatireAlocat,
                recuperareAlocat, total, observatii);
        }

        public async Task DeleteEntryAsync(int? id = null, string date = null)
        {
            await MainSql.DeleteFromDbAsync(false, id);
            await RefreshElementsAsync().ConfigureAwait(false);
        }

        private async Task RefreshElementsAsync() =>
                Entries = await MainSql.GetAllElementsAsync<StandardDatabaseEntry>();

        public int MaxElement() =>
                Entries?.Count ?? 0;
    }
}
