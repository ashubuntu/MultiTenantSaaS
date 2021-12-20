using Microsoft.Azure.SqlDatabase.ElasticScale.Query;

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
            bool userExists = false;

            using (MultiShardConnection conn = new MultiShardConnection(shardDetails.Item2.ShardMap.GetShards(), shardDetails.Item1.ConnectionString))
            {
                using (MultiShardCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM Users WHERE UserEmail = @UserEmail";
                    cmd.Parameters.AddWithValue("@UserEmail", user.UserEmail);

                    cmd.ExecutionPolicy = MultiShardExecutionPolicy.CompleteResults;

                    userExists = ShardHelper.ExecuteCommand<User>(cmd) != null;
                }
            }

            return userExists;
        }
    }
}
