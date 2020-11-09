using System;

namespace CheckinLS.InterfacesAndClasses
{
    internal class GetDate : IGetDate
    {
        public string GetCurrentDate() =>
                DateTime.Now.ToString("yyyy-MM-dd");
    }
}
