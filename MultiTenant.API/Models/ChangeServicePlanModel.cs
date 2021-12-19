namespace MultiTenant.API.Models
{
    public class ChangeServicePlanModel
    {
        public string UserEmail { get; set; }
        public ServicePlan ServicePlan { get; set; }
        public ServicePlan TargetServicePlan { get; set; }
    }
}