using CheckinLS.API.Misc;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace CheckinLS.InterfacesAndClasses.Users
{
    public interface IUsers
    {
        Task CreateUsersCacheAsync(SqlConnection conn);
        List<Accounts> DeserializeCache();
    }
}
