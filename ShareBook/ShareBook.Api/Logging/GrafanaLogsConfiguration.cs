using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.OpenTelemetry;
using System;
using System.Collections.Generic;

namespace ShareBook.Api.Logging;

public static class GrafanaLogsConfiguration
{
    private const string ServiceName = "sharebook-api";

    // GrafanaLogs__Endpoint: URL completa do OTLP/HTTP de logs, ex: https://otlp-gateway-<regiao>.grafana.net/otlp/v1/logs
    // GrafanaLogs__Headers: "Authorization=Basic <base64>" (formato OTEL_EXPORTER_OTLP_HEADERS).
    // Separado da config de metricas de proposito: credencial propria, so de escrita de logs.
    public static LoggerConfiguration AddGrafanaLogs(
        this LoggerConfiguration loggerConfiguration,
        IConfiguration configuration,
        string environment)
    {
        var endpoint = configuration["GrafanaLogs:Endpoint"];
        var headers = ParseHeaders(configuration["GrafanaLogs:Headers"]);

        if (string.IsNullOrWhiteSpace(endpoint) || headers.Count == 0)
            return loggerConfiguration;

        return loggerConfiguration.WriteTo.OpenTelemetry(options =>
        {
            options.Endpoint = endpoint;
            options.Protocol = OtlpProtocol.HttpProtobuf;
            options.Headers = headers;
            options.RestrictedToMinimumLevel = LogEventLevel.Information;
            options.ResourceAttributes = new Dictionary<string, object>
            {
                ["service.name"] = ServiceName,
                ["deployment.environment"] = environment,
            };
        });
    }

    public static Dictionary<string, string> ParseHeaders(string? raw)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(raw))
            return headers;

        foreach (var pair in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            // Divide so no primeiro '=': o base64 do token pode terminar em '='.
            var separator = pair.IndexOf('=');
            if (separator <= 0)
                continue;

            headers[pair[..separator].Trim()] = pair[(separator + 1)..].Trim();
        }

        return headers;
    }
}
