using System.Collections.Generic;

namespace MultiTenant.Service.Repositories
{
    public interface IUserRepository
    {
        bool CreateUser(User user, dynamic shardDetails);
        bool DeleteUser(User user, dynamic shardDetails);
        bool UserExists(User user, dynamic shardDetails, bool checkTenant = false);
        IEnumerable<User> GetAll(int tenantId, dynamic shardDetails);
    }
}
