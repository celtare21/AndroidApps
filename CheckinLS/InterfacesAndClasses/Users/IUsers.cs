using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace CheckinLS.InterfacesAndClasses.Users
{
    public interface IUsers
    {
        Task CreateUsersCacheAsync(SqlConnection conn);
        Task<Dictionary<string, string>> DeserializeCacheAsync();
        Task CreateLoggedUserAsync(string pin);
        Task<string> ReadLoggedUserAsync();
        Users.UserHelpers GetHelpers();
    }
}
