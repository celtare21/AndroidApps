using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace CheckinLS.InterfacesAndClasses.Users
{
    public interface IUsers
    {
        Task CreateUsersCacheAsync(SqlConnection conn);
        Dictionary<string, string> DeserializeCache();
    }
}
