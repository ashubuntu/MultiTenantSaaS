using System.ComponentModel.DataAnnotations;

namespace MultiTenant.Service
{
    public class User
    {
        [Key]
        public string UserEmail { get; set; }
        public int TenantId { get; set; }
    }
}