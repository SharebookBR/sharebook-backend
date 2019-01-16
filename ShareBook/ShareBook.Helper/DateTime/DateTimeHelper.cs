using System;

namespace ShareBook.Helper
{
    static public class DateTimeHelper
    {
        static public TimeSpan GetTimeNowSaoPaulo()
        {
            var now = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"));
            var today = new DateTime(now.Year, now.Month, now.Day);
            return now - today;
        }
    }
}
