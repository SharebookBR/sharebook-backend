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

        // data-hora de hoje a meia noite.
        static public DateTime GetTodaySaoPaulo()
        {
            var nowSP = GetTimeNowSaoPaulo();
            var todaySP = DateTime.Now.AddHours(nowSP.Hours * -1).AddMinutes(nowSP.Minutes * -1).AddSeconds(nowSP.Seconds * -1);
            return todaySP;
        }

        static public DateTime ConvertDateTimeSaoPaulo(DateTime d) => TimeZoneInfo.ConvertTime(d, TimeZoneInfo.FindSystemTimeZoneById(SaoPauloTimezoneId));
    }
}
