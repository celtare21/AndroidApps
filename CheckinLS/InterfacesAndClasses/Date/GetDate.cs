using System;
using Xamarin.Forms.Xaml;

namespace CheckinLS.InterfacesAndClasses.Date
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class GetDate : IGetDate
    {
        public DateTime GetCurrentDate() =>
            DateTime.Now;
    }
}
