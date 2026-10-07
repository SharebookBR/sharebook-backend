using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace ShareBook.Api.Configuration;

public static class OpenTelemetryConfiguration
{
    private const string ServiceName = "sharebook-api";

    public static IServiceCollection AddShareBookOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var endpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        var headers = configuration["OTEL_EXPORTER_OTLP_HEADERS"];

        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(headers))
        {
            return services;
        }

        services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                serviceName: ServiceName,
                serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString()))
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter(
                        "Microsoft.AspNetCore.Hosting",
                        "Microsoft.AspNetCore.Server.Kestrel",
                        "System.Net.Http",
                        "System.Runtime")
                    .AddOtlpExporter();
            });

        return services;
    }
}
