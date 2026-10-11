using Microsoft.AspNetCore.Http;
using Serilog.Events;
using System;

namespace ShareBook.Api.Logging;

public static class RequestLogLevel
{
    private static readonly string[] HealthCheckPaths = ["/api/Operations/Ping", "/hc", "/health"];

    // Health checks bem-sucedidos (Docker bate no Ping a cada 30s) viram Verbose e somem
    // dos sinks. Falha (5xx) ou excecao continuam visiveis.
    public static LogEventLevel Get(HttpContext httpContext, double elapsedMs, Exception? ex)
    {
        var failed = ex != null || httpContext.Response.StatusCode > 499;
        if (failed)
            return LogEventLevel.Error;

        return IsHealthCheck(httpContext.Request.Path) ? LogEventLevel.Verbose : LogEventLevel.Information;
    }

    private static bool IsHealthCheck(PathString path) =>
        Array.Exists(HealthCheckPaths, p => path.Equals(p, StringComparison.OrdinalIgnoreCase));
}
