using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace MultiTenant.Service
{
    public static class ExtensionMethods
    {
        public static IDictionary<string, string> ToDictionary(this NameValueCollection source)
        {
            return source?.AllKeys.ToDictionary(k => k, k => source[k]);
        }
    }
}
