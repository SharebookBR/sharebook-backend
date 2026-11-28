using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ShareBook.Domain;
using ShareBook.Repository;
using ShareBook.Service;

namespace ShareBook.Service.Onboarding;

/// <summary>
/// Lê o estado operacional e os resultados das remessas de onboarding legado.
/// </summary>
public class LegacyOnboardingRolloutService(
    ApplicationDbContext context,
    TimeProvider timeProvider,
    IOptions<LegacyOnboardingBackfillSettings> settings) : ILegacyOnboardingRolloutService
{
    private const string JobName = "LegacyOnboardingBackfill";
    private const int ComparisonMaturityDays = 7;

    private readonly ApplicationDbContext _context = context;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly LegacyOnboardingBackfillSettings _settings = settings.Value;

    public async Task<LegacyOnboardingRolloutDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var batchSize = Math.Max(1, _settings.BatchSize);
        var batches = await _context.LegacyOnboardingBatches
            .AsNoTracking()
            .OrderBy(batch => batch.SequenceNumber)
            .ToListAsync(cancellationToken);

        var journeys = await _context.OnboardingJourneys
            .AsNoTracking()
            .Where(journey => journey.LegacyOnboardingBatchId != null)
            .Select(journey => new JourneyData(
                journey.Id,
                journey.LegacyOnboardingBatchId!.Value,
                journey.CompletedAtUtc,
                journey.CanceledAtUtc))
            .ToListAsync(cancellationToken);
        var deliveries = await _context.OnboardingEmailDeliveries
            .AsNoTracking()
            .Include(delivery => delivery.Clicks)
            .Where(delivery => _context.OnboardingJourneys.Any(journey =>
                journey.Id == delivery.OnboardingJourneyId && journey.LegacyOnboardingBatchId != null))
            .ToListAsync(cancellationToken);

        var lastSuccessfulRunAtUtc = await _context.JobHistories
            .AsNoTracking()
            .Where(history => history.JobName == JobName && history.IsSuccess)
            .OrderByDescending(history => history.CreationDate)
            .Select(history => history.CreationDate)
            .FirstOrDefaultAsync(cancellationToken);
        var cursor = await GetCursorAsync(cancellationToken);
        var remainingEligibleJourneys = await CountRemainingEligibleJourneysAsync(cursor, cancellationToken);
        var remainingBatches = (int)Math.Ceiling(remainingEligibleJourneys / (double)batchSize);
        var nextBatchAtUtc = lastSuccessfulRunAtUtc?.AddDays(15) ?? now;
        var lastSequenceNumber = batches.LastOrDefault()?.SequenceNumber ?? 0;

        var dashboard = new LegacyOnboardingRolloutDashboardDto
        {
            BatchSize = batchSize,
            RemainingEligibleJourneys = remainingEligibleJourneys,
            RemainingBatches = remainingBatches,
            NextBatchAtUtc = remainingBatches > 0 ? nextBatchAtUtc : null,
            EstimatedFinalBatchAtUtc = remainingBatches > 0
                ? nextBatchAtUtc.AddDays((remainingBatches - 1) * 15)
                : null
        };

        foreach (var batch in batches)
        {
            var batchJourneys = journeys.Where(journey => journey.BatchId == batch.Id).ToList();
            var journeyIds = batchJourneys.Select(journey => journey.Id).ToHashSet();
            var batchDeliveries = deliveries
                .Where(delivery => journeyIds.Contains(delivery.OnboardingJourneyId))
                .ToList();
            var matureAtUtc = batch.ExecutedAtUtc.AddDays(ComparisonMaturityDays);
            var isComparable = now >= matureAtUtc;

            dashboard.Batches.Add(new LegacyOnboardingBatchDto
            {
                Id = batch.Id,
                SequenceNumber = batch.SequenceNumber,
                ExecutedAtUtc = batch.ExecutedAtUtc,
                EnrolledCount = batch.EnrolledCount,
                CurrentJourneys = batchJourneys.Count,
                NewestUserCreationAtUtc = batch.NewestUserCreationAtUtc,
                OldestUserCreationAtUtc = batch.OldestUserCreationAtUtc,
                AgeInDays = Math.Max(0, (int)Math.Floor((now - batch.ExecutedAtUtc).TotalDays)),
                IsComparable = isComparable,
                CurrentMetrics = Metrics(batchJourneys, batchDeliveries, null),
                FirstSevenDaysMetrics = isComparable
                    ? Metrics(batchJourneys, batchDeliveries, matureAtUtc)
                    : null
            });
        }

        for (var index = 0; index < remainingBatches; index++)
        {
            var estimatedJourneys = Math.Min(batchSize, remainingEligibleJourneys - (index * batchSize));
            dashboard.UpcomingBatches.Add(new LegacyOnboardingUpcomingBatchDto
            {
                SequenceNumber = lastSequenceNumber + index + 1,
                ScheduledAtUtc = nextBatchAtUtc.AddDays(index * 15),
                EstimatedJourneys = estimatedJourneys
            });
        }

        return dashboard;
    }

    private async Task<int> CountRemainingEligibleJourneysAsync(Cursor? cursor, CancellationToken cancellationToken)
    {
        var emails = await EligibleUsersAfterCursor(cursor)
            .Select(user => user.Email)
            .ToListAsync(cancellationToken);

        return emails.Count(EmailAddressValidator.IsValid);
    }

    private IQueryable<User> EligibleUsersAfterCursor(Cursor? cursor)
    {
        var query = _context.Users
            .AsNoTracking()
            .Where(user => user.Active
                && user.ParentAproved
                && user.AllowSendingEmail
                && user.CreationDate.HasValue
                && user.CreationDate < _settings.EligibleBeforeUtc
                && !_context.OnboardingJourneys.Any(journey => journey.UserId == user.Id)
                && !_context.MailBounces.Any(bounce => bounce.Email == user.Email && !bounce.IsSoft));

        if (cursor is null)
            return query;

        return query.Where(user => user.CreationDate < cursor.CreationDate
            || (user.CreationDate == cursor.CreationDate && user.Id.CompareTo(cursor.UserId) < 0));
    }

    private async Task<Cursor?> GetCursorAsync(CancellationToken cancellationToken)
    {
        var lastResult = await _context.JobHistories
            .AsNoTracking()
            .Where(history => history.JobName == JobName && history.IsSuccess && history.LastResult != null)
            .OrderByDescending(history => history.CreationDate)
            .Select(history => history.LastResult)
            .FirstOrDefaultAsync(cancellationToken);

        return Cursor.TryParse(lastResult, out var cursor) ? cursor : null;
    }

    private static LegacyOnboardingBatchMetricsDto Metrics(
        IReadOnlyCollection<JourneyData> journeys,
        IReadOnlyCollection<OnboardingEmailDelivery> deliveries,
        DateTime? untilUtc)
    {
        var firstEmailDeliveries = deliveries
            .Where(delivery => delivery.Step == 0 && (!untilUtc.HasValue || delivery.QueuedAtUtc <= untilUtc.Value))
            .ToList();
        return new LegacyOnboardingBatchMetricsDto
        {
            FirstEmailQueued = firstEmailDeliveries.Count,
            FirstEmailOpened = firstEmailDeliveries.Count(delivery => delivery.FirstOpenedAtUtc.HasValue),
            FirstEmailClicked = firstEmailDeliveries.Count(delivery => delivery.Clicks.Count > 0),
            JourneysCompleted = journeys.Count(journey => journey.CompletedAtUtc.HasValue && (!untilUtc.HasValue || journey.CompletedAtUtc <= untilUtc.Value)),
            JourneysCanceled = journeys.Count(journey => journey.CanceledAtUtc.HasValue && (!untilUtc.HasValue || journey.CanceledAtUtc <= untilUtc.Value))
        };
    }

    private sealed record JourneyData(Guid Id, Guid BatchId, DateTime? CompletedAtUtc, DateTime? CanceledAtUtc);

    private sealed record Cursor(DateTime CreationDate, Guid UserId)
    {
        public static bool TryParse(string? value, out Cursor? cursor)
        {
            cursor = null;
            var parts = value?.Split('|');
            if (parts?.Length != 2
                || !DateTime.TryParse(parts[0], null, System.Globalization.DateTimeStyles.RoundtripKind, out var creationDate)
                || !Guid.TryParse(parts[1], out var userId))
                return false;

            cursor = new Cursor(creationDate, userId);
            return true;
        }
    }
}
