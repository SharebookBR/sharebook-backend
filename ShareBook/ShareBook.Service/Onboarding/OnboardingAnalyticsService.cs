using Microsoft.EntityFrameworkCore;
using ShareBook.Repository;

namespace ShareBook.Service.Onboarding;

public class OnboardingAnalyticsService(
    ApplicationDbContext context,
    TimeProvider timeProvider) : IOnboardingAnalyticsService
{
    private readonly ApplicationDbContext _context = context;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<OnboardingAnalyticsDashboardDto> GetDashboardAsync(int days, CancellationToken cancellationToken = default)
    {
        var normalizedDays = days is 7 or 30 or 90 ? days : 30;
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var since = now.AddDays(-normalizedDays);

        var deliveries = await _context.OnboardingEmailDeliveries
            .AsNoTracking()
            .Include(delivery => delivery.Clicks)
            .Where(delivery => delivery.QueuedAtUtc >= since)
            .ToListAsync(cancellationToken);

        var journeys = _context.OnboardingJourneys.AsNoTracking();
        var dashboard = new OnboardingAnalyticsDashboardDto
        {
            Days = normalizedDays,
            JourneysStarted = await journeys.CountAsync(journey => journey.CreationDate >= since, cancellationToken),
            JourneysCompleted = await journeys.CountAsync(journey => journey.CompletedAtUtc >= since, cancellationToken),
            JourneysCanceled = await journeys.CountAsync(journey => journey.CanceledAtUtc >= since, cancellationToken),
            OverdueJourneys = await journeys.CountAsync(journey =>
                journey.CompletedAtUtc == null && journey.CanceledAtUtc == null && journey.NextEmailAtUtc <= now,
                cancellationToken)
        };

        for (var step = 0; step < 5; step++)
        {
            var stepDeliveries = deliveries.Where(delivery => delivery.Step == step).ToList();
            dashboard.Steps.Add(new OnboardingStepMetricDto
            {
                Step = step,
                Label = StepLabel(step),
                Queued = stepDeliveries.Count,
                Opened = stepDeliveries.Count(delivery => delivery.FirstOpenedAtUtc.HasValue),
                Clicked = stepDeliveries.Count(delivery => delivery.Clicks.Count > 0),
                Ctas = stepDeliveries
                    .SelectMany(delivery => delivery.Clicks)
                    .GroupBy(click => click.Cta)
                    .OrderBy(group => group.Key)
                    .Select(group => new OnboardingCtaMetricDto { Cta = group.Key, Clicks = group.Count() })
                    .ToList()
            });
        }

        return dashboard;
    }

    private static string StepLabel(int step) => step switch
    {
        0 => "1. Boas-vindas",
        1 => "2. Catálogo",
        2 => "3. Impacto da doação",
        3 => "4. Como funciona",
        4 => "5. Comunidade",
        _ => string.Empty
    };
}
