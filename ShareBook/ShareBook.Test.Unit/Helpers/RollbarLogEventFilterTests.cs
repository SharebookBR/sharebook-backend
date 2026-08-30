using Serilog.Events;
using Serilog.Parsing;
using ShareBook.Api.Logging;
using ShareBook.Service;
using System;
using Xunit;

namespace ShareBook.Test.Unit.Helpers
{
    public class RollbarLogEventFilterTests
    {
        [Fact]
        public void ExcludesLowLevelEntityFrameworkCommandErrors()
        {
            var logEvent = CreateLogEvent(
                "Microsoft.EntityFrameworkCore.Database.Command",
                new Exception("database command failed"));

            Assert.True(RollbarLogEventFilter.ShouldExclude(logEvent));
        }

        [Fact]
        public void ExcludesExpectedDuplicateUserEmailUpdateError()
        {
            var exception = new Exception(
                "23505: duplicate key value violates unique constraint \"idx_17678_IX_Users_Email\"");
            var logEvent = CreateLogEvent("Microsoft.EntityFrameworkCore.Update", exception);

            Assert.True(RollbarLogEventFilter.ShouldExclude(logEvent));
            Assert.True(DuplicateEmailExceptionDetector.IsDuplicateEmail(exception));
        }

        [Fact]
        public void KeepsOtherEntityFrameworkUpdateErrors()
        {
            var exception = new Exception(
                "23505: duplicate key value violates unique constraint \"IX_Books_Slug\"");
            var logEvent = CreateLogEvent("Microsoft.EntityFrameworkCore.Update", exception);

            Assert.False(RollbarLogEventFilter.ShouldExclude(logEvent));
            Assert.False(DuplicateEmailExceptionDetector.IsDuplicateEmail(exception));
        }

        [Fact]
        public void KeepsApplicationErrors()
        {
            var logEvent = CreateLogEvent(
                "ShareBook.Api.Middleware.ExceptionHandlerMiddleware",
                new InvalidOperationException("unexpected"));

            Assert.False(RollbarLogEventFilter.ShouldExclude(logEvent));
        }

        private static LogEvent CreateLogEvent(string sourceContext, Exception exception)
        {
            return new LogEvent(
                DateTimeOffset.UtcNow,
                LogEventLevel.Error,
                exception,
                new MessageTemplateParser().Parse("Test event"),
                [new LogEventProperty("SourceContext", new ScalarValue(sourceContext))]);
        }
    }
}
