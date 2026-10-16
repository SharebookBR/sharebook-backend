using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShareBook.Service.Analytics;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShareBook.Api.HostedServices;

public sealed class AnalyticsDashboardWarmupHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<AnalyticsDashboardWarmupHostedService> logger) : BackgroundService
{
    // 03:00 UTC = midnight in the Sharebook's primary Brazil timezone.
    private static readonly TimeSpan ScheduledWarmupTimeUtc = new(3, 0, 0);

    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<AnalyticsDashboardWarmupHostedService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await WarmAsync("startup", stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = GetDelayUntilNextScheduledWarmup(DateTimeOffset.UtcNow);
            _logger.LogInformation(
                "Analytics dashboard next scheduled warmup at {ScheduledAtUtc}.",
                DateTimeOffset.UtcNow.Add(delay));

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            await WarmAsync("scheduled", stoppingToken);
        }
    }

    private async Task WarmAsync(string source, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var analyticsService = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();
            await analyticsService.WarmCacheAsync(source, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Analytics dashboard warmup cancelled. Source={Source}",
                source);
        }
        catch (Exception exception)
        {
            // A warmup failure must not terminate the API host.
            _logger.LogError(
                exception,
                "Analytics dashboard warmup failed. Source={Source}",
                source);
        }
    }

    private static TimeSpan GetDelayUntilNextScheduledWarmup(DateTimeOffset nowUtc)
    {
        var nextWarmup = nowUtc.UtcDateTime.Date.Add(ScheduledWarmupTimeUtc);

        if (nextWarmup <= nowUtc.UtcDateTime)
            nextWarmup = nextWarmup.AddDays(1);

        return nextWarmup - nowUtc.UtcDateTime;
    }
}
