using Microsoft.Azure.SqlDatabase.ElasticScale.Query;
using System.Collections.Generic;
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

        public IEnumerable<User> GetAll(int tenantId, dynamic shardDetails)
        {
            IEnumerable<User> users = null;
            SqlDatabaseUtils.SqlRetryPolicy.ExecuteAction(() =>
            {
                using (var db = new ElasticScaleContext<int>(shardDetails.Item2.ShardMap, tenantId, shardDetails.Item1.ConnectionString))
                {
                    var query = from u in db.Users
                                where u.TenantId == tenantId
                                select u;
                    users = query.ToList();
                }
            });

            return users;
        }

        public bool UserExists(User user, dynamic shardDetails, bool checkTenant = false)
        {
            bool userExists = false;

            using (MultiShardConnection conn = new MultiShardConnection(shardDetails.Item2.ShardMap.GetShards(), shardDetails.Item1.ConnectionString))
            {
                using (MultiShardCommand cmd = conn.CreateCommand())
                {
                    var appendTenantCond = checkTenant ? " AND TenantId = @TenantId" : string.Empty;
                    cmd.CommandText = $"SELECT * FROM Users WHERE UserEmail = @UserEmail {appendTenantCond}";
                    cmd.Parameters.AddWithValue("@UserEmail", user.UserEmail);
                    if (checkTenant)
                    {
                        cmd.Parameters.AddWithValue("@TenantId", user.TenantId);
                    }

                    cmd.ExecutionPolicy = MultiShardExecutionPolicy.CompleteResults;

                    var result = ShardHelper.ExecuteCommand<User>(cmd);
                    userExists = result != null;
                }
            }

            return userExists;
        }
    }
}
