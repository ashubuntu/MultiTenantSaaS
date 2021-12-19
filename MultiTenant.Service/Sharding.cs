using System.Data.SqlClient;
using System.Linq;
using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;

namespace MultiTenant.Service
{
    public class Sharding
    {
        public ShardMapManager ShardMapManager { get; private set; }
        public ListShardMap<int> ShardMap { get; private set; }

        // Bootstrap Elastic Scale by creating a new shard map manager and a shard map on 
        // the shard map manager database if necessary.
        public Sharding(string smmserver, string smmdatabase, string smmconnstr)
        {
            // Connection string with administrative credentials for the root database
            SqlConnectionStringBuilder connStrBldr = new SqlConnectionStringBuilder(smmconnstr)
            {
                DataSource = smmserver,
                InitialCatalog = smmdatabase
            };

            // Deploy shard map manager.
            if (!ShardMapManagerFactory.TryGetSqlShardMapManager(connStrBldr.ConnectionString, ShardMapManagerLoadPolicy.Lazy, out ShardMapManager smm))
            {
                ShardMapManager = ShardMapManagerFactory.CreateSqlShardMapManager(connStrBldr.ConnectionString);
            }
            else
            {
                ShardMapManager = smm;
            }

            if (!ShardMapManager.TryGetListShardMap("MultiTenantSaaS", out ListShardMap<int> sm))
            {
                ShardMap = ShardMapManager.CreateListShardMap<int>("MultiTenantSaaS");
            }
            else
            {
                ShardMap = sm;
            }
        }

        // Enter a new shard - i.e. an empty database - to the shard map, allocate a first tenant to it 
        // and kick off EF intialization of the database to deploy schema
        public void RegisterNewShard(string server, string database, string connstr, int key)
        {
            ShardLocation shardLocation = new ShardLocation(server, database);

            if (!ShardMap.TryGetShard(shardLocation, out Shard shard))
            {
                shard = ShardMap.CreateShard(shardLocation);
            }

            SqlConnectionStringBuilder connStrBldr = new SqlConnectionStringBuilder(connstr)
            {
                DataSource = server,
                InitialCatalog = database
            };

            // Go into a DbContext to trigger migrations and schema deployment for the new shard.
            // This requires an un-opened connection.
            using (var db = new ElasticScaleContext<int>(connStrBldr.ConnectionString))
            {
                // Run a query to engage EF migrations
                (from u in db.Users
                 select u).Count();
            }

            // Register the mapping of the tenant to the shard in the shard map.
            // After this step, DDR on the shard map can be used
            if (!ShardMap.TryGetMappingForKey(key, out PointMapping<int> mapping))
            {
                ShardMap.CreatePointMapping(key, shard);
            }
        }
    }
}
