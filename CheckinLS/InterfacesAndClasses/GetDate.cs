using System;

namespace CheckinLS.InterfacesAndClasses
{
    internal class GetDate : IGetDate
    {
        public DateTime GetCurrentDate() =>
                DateTime.Now;
    }
}
