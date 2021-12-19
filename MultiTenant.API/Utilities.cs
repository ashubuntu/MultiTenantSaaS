using System.Text.RegularExpressions;

namespace MultiTenant.API
{
    public class Utilities
    {
        public static string SplitCamelCase(string input)
        {
            return Regex.Replace(input, "([A-Z])", " $1", RegexOptions.Compiled).Trim();
        }
    }
}
