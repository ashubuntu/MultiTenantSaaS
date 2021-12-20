using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MultiTenant.API.Models
{
    public class UserModel
    {
        public string UserEmail { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ServicePlan ServicePlan { get; set; }
    }
}