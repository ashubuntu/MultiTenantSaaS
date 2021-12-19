using MultiTenant.API.Configuration;
using MultiTenant.API.ExceptionHandling;
using System;
using System.Web;
using System.Web.Http;

namespace MultiTenant.API
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            GlobalConfiguration.Configure(APIConfig.Register);
            GlobalConfiguration.Configuration.Filters.Add(new LogExceptionFilterAttribute());
        }
    }
}
