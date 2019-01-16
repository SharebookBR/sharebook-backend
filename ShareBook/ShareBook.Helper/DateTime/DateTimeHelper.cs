using System;

namespace ShareBook.Helper
{
    static public class DateTimeHelper
    {
        static private readonly string SaoPauloTimezoneId = "E. South America Standard Time";

        static public TimeSpan GetTimeNowSaoPaulo()
        {
            var now = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById(SaoPauloTimezoneId));
            var today = new DateTime(now.Year, now.Month, now.Day);
            return now - today;
        }

        static public DateTime ConvertDateTimeSaoPaulo(DateTime d) => TimeZoneInfo.ConvertTime(d, TimeZoneInfo.FindSystemTimeZoneById(SaoPauloTimezoneId));
    }
}
