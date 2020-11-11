using CheckinLS.API;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CheckinLS.InterfacesAndClasses
{
    public interface IUsers
    {
        Task CreateUsersCacheAsync();
        List<Accounts> DeserializeCache();
    }
}
