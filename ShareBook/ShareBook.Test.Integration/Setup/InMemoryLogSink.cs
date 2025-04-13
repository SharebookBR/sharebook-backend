using Serilog.Core;
using Serilog.Events;

namespace ShareBook.Test.Integration.Setup
{
    public class InMemoryLogSink : ILogEventSink
    {
        public List<LogEvent> Events { get; } = new List<LogEvent>();

        public void Emit(LogEvent logEvent)
        {
            Events.Add(logEvent);
        }
    }
}
