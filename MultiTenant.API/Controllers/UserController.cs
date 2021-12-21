using MultiTenant.API.Configuration;
using MultiTenant.API.Mapping;
using MultiTenant.API.Models;
using MultiTenant.Service;
using MultiTenant.Service.Repositories;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;

namespace MultiTenant.API.Controllers
{
    public class UserController : BaseController
    {
        private readonly dynamic appSettings = new AppSettingsWrapper();
        private readonly IUserRepository userRepository = new UserRepository();

        // POST: api/User/Register
        public IHttpActionResult Register(UserModel registerModel)
        {
            var tenantId = (int)registerModel.ServicePlan;
            var shardDetails = ShardHelper.EnsureTenantShard(tenantId, registerModel.ServicePlan.ToString(), appSettings);

            User user = new User() { TenantId = tenantId, UserEmail = registerModel.UserEmail };
            bool userExists = userRepository.UserExists(user, shardDetails);

            if (userExists)
            {
                return GetJsonResult($"user already exists - UserEmail: {registerModel.UserEmail}, Service Plan: {registerModel.ServicePlan}", code: HttpStatusCode.NotModified);
            }

            bool created = userRepository.CreateUser(user, shardDetails);
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
            bool userExists = userRepository.UserExists(user, shardDetails, true);

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

            return GetJsonResult($"cannot change plan - user does not exist in service plan: {changeServicePlanModel.ServicePlan}", code: HttpStatusCode.BadRequest);
        }

        public IHttpActionResult GetAll(ServicePlan ServicePlan)
        {
            var tenantId = (int)ServicePlan;
            var shardDetails = ShardHelper.EnsureTenantShard(tenantId, ServicePlan.ToString(), appSettings);
            var users = userRepository.GetAll(tenantId, shardDetails);

            dynamic content;
            if (users.Count > 0)
                content = ((IEnumerable<User>)users).ToModel();
            else content = $"no users found for Service Plan: {ServicePlan}";

            return GetJsonResult(content, action: "GetAll", code: users.Count > 0 ? HttpStatusCode.Found : HttpStatusCode.NoContent);
        }
    }
}
