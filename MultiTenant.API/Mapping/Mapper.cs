using MultiTenant.API.Models;
using MultiTenant.Service;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiTenant.API.Mapping
{
    public static class Mapper
    {
        public static UserModel ToModel(this User user)
        {
            return new UserModel { UserEmail = user.UserEmail, ServicePlan = (ServicePlan)user.TenantId };
            //return new UserModel { UserEmail = user.UserEmail, ServicePlan = (ServicePlan)Enum.ToObject(typeof(ServicePlan), user.TenantId) };
        }
        public static IEnumerable<UserModel> ToModel(this IEnumerable<User> users)
        {
            return users.Select(user => user.ToModel());
        }
    }
}
