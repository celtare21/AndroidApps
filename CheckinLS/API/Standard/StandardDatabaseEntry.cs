using System;
using Xamarin.Forms.Xaml;

namespace CheckinLS.API.Standard
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class StandardDatabaseEntry
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan OraIncepere { get; set; }
        public TimeSpan OraFinal { get; set; }
        public TimeSpan CursAlocat { get; set; }
        public TimeSpan PregatireAlocat { get; set; }
        public TimeSpan RecuperareAlocat { get; set; }
        public TimeSpan Total { get; set; }
        public string Observatii { get; set; }
    }
}
