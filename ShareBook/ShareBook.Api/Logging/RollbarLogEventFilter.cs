using Serilog.Events;
using ShareBook.Service;

namespace ShareBook.Api.Logging
{
    public static class RollbarLogEventFilter
    {
        private const string SourceContextProperty = "SourceContext";
        private const string EfCommandSource = "Microsoft.EntityFrameworkCore.Database.Command";
        private const string EfUpdateSource = "Microsoft.EntityFrameworkCore.Update";

        public static bool ShouldExclude(LogEvent logEvent)
        {
            ArgumentNullException.ThrowIfNull(logEvent);

            var sourceContext = GetSourceContext(logEvent);

            // CommandError is the low-level copy of an exception that EF or the
            // application logs with the actual cause. Keeping both creates two
            // Rollbar items for one database failure.
            if (sourceContext == EfCommandSource)
                return true;

            return sourceContext == EfUpdateSource &&
                   DuplicateEmailExceptionDetector.IsDuplicateEmail(logEvent.Exception);
        }

        private static string GetSourceContext(LogEvent logEvent)
        {
            if (!logEvent.Properties.TryGetValue(SourceContextProperty, out var value) ||
                value is not ScalarValue scalarValue)
                return null;

            return scalarValue.Value as string;
        }
    }
}
