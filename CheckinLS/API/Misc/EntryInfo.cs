namespace CheckinLS.API.Misc
{
    public class EntryInfo
    {
        public readonly string Obs;
        public readonly bool Curs;
        public readonly bool Pregatire;
        public readonly bool Recuperare;

        public EntryInfo(string obs, bool curs, bool pregatire, bool recuperare)
        {
            Obs = obs;
            Curs = curs;
            Pregatire = pregatire;
            Recuperare = recuperare;
        }
    }
}
