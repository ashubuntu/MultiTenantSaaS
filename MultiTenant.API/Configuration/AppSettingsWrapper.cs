using System.Collections.Specialized;
using System.Configuration;
using System.Dynamic;

namespace MultiTenant.API.Configuration
{
    public class AppSettingsWrapper : DynamicObject
    {
        private readonly NameValueCollection _items;

        public AppSettingsWrapper()
        {
            _items = ConfigurationManager.AppSettings;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = _items[binder.Name];
            return result != null;
        }
    }
}
