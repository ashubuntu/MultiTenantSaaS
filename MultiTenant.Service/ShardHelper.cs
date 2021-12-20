using System.Configuration;
using System.Data.SqlClient;

namespace MultiTenant.Service
{
    public static class ShardHelper
    {
        public static (SqlConnectionStringBuilder, Sharding) EnsureTenantShard(int tenantId, string servicePlan, dynamic appSettings)
        {
            var shardDB = ConfigurationManager.AppSettings[servicePlan];
            SqlConnectionStringBuilder connStrBldr = new SqlConnectionStringBuilder
            {
                UserID = appSettings.s_userName,
                Password = appSettings.s_password,
                ApplicationName = appSettings.s_applicationName
            };

            Sharding sharding = new Sharding(appSettings.s_server, appSettings.s_shardmapmgrdb, connStrBldr.ConnectionString);
            sharding.RegisterNewShard(appSettings.s_server, shardDB, connStrBldr.ConnectionString, tenantId);

            return (connStrBldr, sharding);
        }
    }
}
