using System.Linq;

namespace MultiTenant.Service.Repositories
{
    public class UserRepository : IUserRepository
    {
        public bool CreateUser(User user, dynamic shardDetails)
        {
            bool created = false;

            SqlDatabaseUtils.SqlRetryPolicy.ExecuteAction(() =>
            {
                using (var db = new ElasticScaleContext<int>(shardDetails.Item2.ShardMap, user.TenantId, shardDetails.Item1.ConnectionString))
                {
                    db.Users.Add(user);
                    created = db.SaveChanges() > 0;
                }
            });

            return created;
        }

        public bool DeleteUser(User user, dynamic shardDetails)
        {
            bool deleted = false;

            SqlDatabaseUtils.SqlRetryPolicy.ExecuteAction(() =>
            {
                using (var db = new ElasticScaleContext<int>(shardDetails.Item2.ShardMap, user.TenantId, shardDetails.Item1.ConnectionString))
                {
                    if (!db.Exists(user))
                        db.Users.Attach(user);

                    db.Users.Remove(user);
                    deleted = db.SaveChanges() > 0;
                }
            });

            return deleted;
        }

        public bool UserExists(User user, dynamic shardDetails)
        {
            bool userExits = false;

            SqlDatabaseUtils.SqlRetryPolicy.ExecuteAction(() =>
            {
                using (var db = new ElasticScaleContext<int>(shardDetails.Item2.ShardMap, user.TenantId, shardDetails.Item1.ConnectionString))
                {
                    var query = from u in db.Users
                                where u.TenantId == user.TenantId && u.UserEmail == user.UserEmail
                                select u;

                    if (query.Count() > 0)
                    {
                        userExits = true;
                    }
                }
            });

            return userExits;
        }
    }
}
