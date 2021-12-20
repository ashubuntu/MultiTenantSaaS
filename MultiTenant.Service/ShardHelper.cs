using Microsoft.Azure.SqlDatabase.ElasticScale.Query;
using Newtonsoft.Json;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

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

        /// <summary>
        /// Executes the SQL command and loads the model.
        /// </summary>
        public static T ExecuteCommand<T>(MultiShardCommand cmd)
        {
            dynamic jsonString;
            try
            {
                StringBuilder output = new StringBuilder();
                output.AppendLine();

                using (MultiShardDataReader reader = cmd.ExecuteReader(CommandBehavior.Default))
                {
                    jsonString = JsonConvert.SerializeObject(((NameValueCollection)GetValues(reader).FirstOrDefault()).ToDictionary());
                }
            }
            catch (MultiShardAggregateException)
            {
                jsonString = null;
            }

            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        private static List<dynamic> GetValues(MultiShardDataReader reader)
        {
            if (reader is null)
                return new List<dynamic>();
            var ReturnValue = new List<dynamic>();
            var FieldNames = ArrayPool<string>.Shared.Rent(reader.FieldCount);
            for (var x = 0; x < reader.FieldCount; ++x)
            {
                var FieldName = reader.GetName(x);
                FieldNames[x] = !string.IsNullOrWhiteSpace(FieldName) ? FieldName : $"(No column name #{x})";
            }

            while (reader.Read())
            {
                var Value = new NameValueCollection();
                for (var x = 0; x < reader.FieldCount; ++x)
                {
                    Value.Add(FieldNames[x], reader[x].ToString());
                }

                ReturnValue.Add(Value);
            }

            ArrayPool<string>.Shared.Return(FieldNames);
            return ReturnValue;
        }
    }
}
