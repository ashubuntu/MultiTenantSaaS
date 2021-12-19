using MultiTenant.API.Configuration;
using MultiTenant.API.Models;
using MultiTenant.Service;
using MultiTenant.Service.Repositories;
using System.Net;
using System.Web.Http;

namespace MultiTenant.API.Controllers
{
    public class UserController : BaseController
    {
        private readonly dynamic appSettings = new AppSettingsWrapper();
        private readonly IUserRepository userRepository = new UserRepository();

        // POST: api/User/Register
        public IHttpActionResult Register(RegisterModel registerModel)
        {
            var tenantId = (int)registerModel.ServicePlan;
            var shardDetails = ShardHelper.EnsureTenantShard(tenantId, registerModel.ServicePlan.ToString(), appSettings);

            User user = new User() { TenantId = tenantId, UserEmail = registerModel.UserEmail };
            bool userExists = userRepository.UserExists(user, shardDetails);

            if (userExists)
            {
                return GetJsonResult("user already exists", code: HttpStatusCode.NotModified);
            }

            bool created = userRepository.CreateUser(new User() { TenantId = tenantId, UserEmail = registerModel.UserEmail }, shardDetails);
            if (created)
            {
                return GetJsonResult("user registered", code: HttpStatusCode.Created);
            }

            return GetJsonResult("user not registered", code: HttpStatusCode.BadRequest);
        }

        // POST: api/User/ChangeServicePlan
        public IHttpActionResult ChangeServicePlan(ChangeServicePlanModel changeServicePlanModel)
        {
            var currentTenantId = (int)changeServicePlanModel.ServicePlan;
            var targetTenantId = (int)changeServicePlanModel.TargetServicePlan;
            if (currentTenantId == targetTenantId)
            {
                return GetJsonResult("target plan must be different", code: HttpStatusCode.NotModified);
            }

            // source plan
            var shardDetails = ShardHelper.EnsureTenantShard(currentTenantId, changeServicePlanModel.ServicePlan.ToString(), appSettings);
            User user = new User() { TenantId = currentTenantId, UserEmail = changeServicePlanModel.UserEmail };
            bool userExists = userRepository.UserExists(user, shardDetails);

            if (userExists)
            {
                bool deleted = userRepository.DeleteUser(user, shardDetails);

                // target plan
                if (deleted)
                {
                    shardDetails = ShardHelper.EnsureTenantShard(targetTenantId, changeServicePlanModel.TargetServicePlan.ToString(), appSettings);

                    bool created = userRepository.CreateUser(new User() { TenantId = targetTenantId, UserEmail = changeServicePlanModel.UserEmail }, shardDetails);
                    if (created)
                        return GetJsonResult("service plan changed", code: HttpStatusCode.Moved);
                }
            }

            return GetJsonResult("cannot change plan", code: HttpStatusCode.BadRequest);
        }
    }
}
