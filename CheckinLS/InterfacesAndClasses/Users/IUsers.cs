using CheckinLS.API.Misc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CheckinLS.InterfacesAndClasses.Users
{
    public interface IUsers
    {
        Task CreateUsersCacheAsync();
        List<Accounts> DeserializeCache();
        void DropCache();
    }
}
