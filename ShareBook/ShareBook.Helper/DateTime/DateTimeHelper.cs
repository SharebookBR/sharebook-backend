using System;

namespace ShareBook.Helper
{
    static public class DateTimeHelper
    {
        static private readonly string SaoPauloTimezoneId = "E. South America Standard Time";

        // hora agora.
        static public TimeSpan GetTimeNowSaoPaulo()
        {
            var now = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById(SaoPauloTimezoneId));
            var today = new DateTime(now.Year, now.Month, now.Day);
            return now - today;
        }

        // data hora agora.
        static public DateTime GetDateTimeNowSaoPaulo() => TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById(SaoPauloTimezoneId));
 
        // data-hora de hoje a meia noite.
        static public DateTime GetTodaySaoPaulo()
        {
            var nowSP = GetDateTimeNowSaoPaulo();
            var todaySP = new DateTime(nowSP.Year, nowSP.Month, nowSP.Day, 0, 0, 0);
            return todaySP;
        }

        static public DateTime ConvertDateTimeSaoPaulo(DateTime d) => TimeZoneInfo.ConvertTime(d, TimeZoneInfo.FindSystemTimeZoneById(SaoPauloTimezoneId));
    }
}
