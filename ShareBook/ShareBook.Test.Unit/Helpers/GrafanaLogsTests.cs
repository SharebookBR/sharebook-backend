using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using ShareBook.Api.Logging;
using ShareBook.Api.Middleware;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace ShareBook.Test.Unit.Helpers;

public class GrafanaLogsTests
{
    [Fact]
    public void ParseHeaders_KeepsEqualsSignsInsideTheValue()
    {
        var headers = GrafanaLogsConfiguration.ParseHeaders("Authorization=Basic abc123==, X-Other=1");

        Assert.Equal("Basic abc123==", headers["Authorization"]);
        Assert.Equal("1", headers["X-Other"]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("sem-igual")]
    public void ParseHeaders_ReturnsEmptyForMissingOrInvalidInput(string? raw)
    {
        Assert.Empty(GrafanaLogsConfiguration.ParseHeaders(raw));
    }

    [Fact]
    public async Task ClientIpMiddleware_AddsIpAndUserAgentToLogsInsideTheRequest()
    {
        var sink = new CollectingSink();
        var logger = new LoggerConfiguration().Enrich.FromLogContext().WriteTo.Sink(sink).CreateLogger();
        var middleware = new ClientIpLogContextMiddleware(_ =>
        {
            logger.Information("dentro da requisicao");
            return Task.CompletedTask;
        });
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.7");
        context.Request.Headers.UserAgent = "TestBot/1.0";

        await middleware.Invoke(context);
        logger.Information("fora da requisicao");

        var inside = sink.Events[0];
        Assert.Equal("\"203.0.113.7\"", inside.Properties["ClientIp"].ToString());
        Assert.Equal("\"TestBot/1.0\"", inside.Properties["UserAgent"].ToString());
        Assert.False(sink.Events[1].Properties.ContainsKey("ClientIp"));
    }

    private class CollectingSink : ILogEventSink
    {
        public List<LogEvent> Events { get; } = [];
        public void Emit(LogEvent logEvent) => Events.Add(logEvent);
    }
}
