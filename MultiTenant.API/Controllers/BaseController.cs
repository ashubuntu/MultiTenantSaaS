using System.Net;
using System.Runtime.CompilerServices;
using System.Web.Http;

namespace MultiTenant.API.Controllers
{
    public class BaseController : ApiController
    {
        public IHttpActionResult GetJsonResult(object content, [CallerMemberName] string action = "", HttpStatusCode code = HttpStatusCode.OK)
        {
            return Json(new
            {
                action,
                code,
                status = Utilities.SplitCamelCase(code.ToString()).ToLower(),
                content
            });
        }
    }
}
