using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;

namespace MultiTenant.API.ExceptionHandling
{
    public class LogExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            context.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(
                    JsonConvert.SerializeObject(
                        new
                        {
                            code = HttpStatusCode.InternalServerError,
                            status = HttpStatusCode.InternalServerError.ToString(),
                            message = "unable to process the request"
                        },
                        Formatting.Indented
                    )
                )
            };
        }
    }
}
