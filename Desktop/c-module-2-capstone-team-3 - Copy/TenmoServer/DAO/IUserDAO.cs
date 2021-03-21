using System.Collections.Generic;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public interface IUserDAO
    {
        ApiUser GetApiUser(string username);
        ApiUser AddApiUser(string username, string password);
        List<ApiUser> GetApiUsers();
        List<User> GetUsers();
        User GetUser(int userId);
    }
}
