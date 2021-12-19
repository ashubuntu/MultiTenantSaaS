namespace MultiTenant.API.Models
{
    public class RegisterModel
    {
        public string UserEmail { get; set; }
        public ServicePlan ServicePlan { get; set; }
    }
}