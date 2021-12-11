using System;
using System.Globalization;
using System.Reflection;

namespace ShareBook.Helper.Extensions
{
    public static class AssemblyExtension
    {
        public static DateTime GetLinkerTime(this Assembly ass)
        {
            const string BuildVersionPrefix = "+build";

            var attribute = ass.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (attribute?.InformationalVersion != null)
            {
                var value = attribute.InformationalVersion;
                var index = value.IndexOf(BuildVersionPrefix);
                if (index > 0)
                {
                    value = value[(index + BuildVersionPrefix.Length)..];
                    return DateTime.ParseExact(value, "yyyy-MM-ddTHH:mm:ss:fffZ", CultureInfo.InvariantCulture);
                }
            }
            return default;
        }
    }
}
