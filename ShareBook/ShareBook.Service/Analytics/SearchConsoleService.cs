namespace ShareBook.Service.Analytics;

public class SearchConsoleService : ISearchConsoleService
{
    private const int FinalDataLagDays = 3;
    private const int PeriodDays = 28;
    private const int OpportunityRowLimit = 25_000;
    private const double MinimumOpportunityImpressions = 20;
    private const double MaximumOpportunityCtr = 0.05;
    private const double MaximumOpportunityPosition = 20;

    private readonly ISearchConsoleApiClient _apiClient;
    private readonly TimeProvider _timeProvider;

    public SearchConsoleService(
        ISearchConsoleApiClient apiClient,
        TimeProvider timeProvider)
    {
        _apiClient = apiClient;
        _timeProvider = timeProvider;
    }

    public async Task<SearchConsoleAnalytics> GetOverviewAsync(
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);
        var currentEnd = today.AddDays(-FinalDataLagDays);
        var currentStart = currentEnd.AddDays(-(PeriodDays - 1));
        var previousEnd = currentStart.AddDays(-1);
        var previousStart = previousEnd.AddDays(-(PeriodDays - 1));

        var currentTask = _apiClient.QueryAsync(
            new SearchConsoleApiQuery(currentStart, currentEnd, []),
            cancellationToken);
        var previousTask = _apiClient.QueryAsync(
            new SearchConsoleApiQuery(previousStart, previousEnd, []),
            cancellationToken);
        var dailyTask = _apiClient.QueryAsync(
            new SearchConsoleApiQuery(currentStart, currentEnd, ["date"], PeriodDays),
            cancellationToken);
        var opportunitiesTask = _apiClient.QueryAsync(
            new SearchConsoleApiQuery(
                currentStart,
                currentEnd,
                ["query", "page"],
                OpportunityRowLimit),
            cancellationToken);

        await Task.WhenAll(currentTask, previousTask, dailyTask, opportunitiesTask);

        return new SearchConsoleAnalytics
        {
            Available = true,
            StartDate = currentStart.ToString("yyyy-MM-dd"),
            EndDate = currentEnd.ToString("yyyy-MM-dd"),
            Current = ToSummary((await currentTask).Rows.FirstOrDefault()),
            Previous = ToSummary((await previousTask).Rows.FirstOrDefault()),
            Daily = (await dailyTask).Rows
                .Where(row => row.Keys.Count > 0 && DateOnly.TryParse(row.Keys[0], out _))
                .Select(row => new SearchConsoleDailyMetric
                {
                    Date = row.Keys[0],
                    Clicks = row.Clicks,
                    Impressions = row.Impressions
                })
                .OrderBy(row => row.Date, StringComparer.Ordinal)
                .ToList(),
            Opportunities = SelectOpportunities((await opportunitiesTask).Rows)
        };
    }

    private static SearchConsoleMetricSummary ToSummary(SearchConsoleApiRow row)
    {
        if (row is null)
            return new SearchConsoleMetricSummary();

        return new SearchConsoleMetricSummary
        {
            Clicks = row.Clicks,
            Impressions = row.Impressions,
            Ctr = row.Ctr,
            Position = row.Position
        };
    }

    private static List<SearchConsoleOpportunity> SelectOpportunities(
        IEnumerable<SearchConsoleApiRow> rows)
    {
        return rows
            .Where(row => row.Keys.Count >= 2)
            .Where(row => row.Impressions >= MinimumOpportunityImpressions)
            .Where(row => row.Ctr < MaximumOpportunityCtr)
            .Where(row => row.Position <= MaximumOpportunityPosition)
            .OrderByDescending(row => row.Impressions * (MaximumOpportunityCtr - row.Ctr))
            .ThenByDescending(row => row.Impressions)
            .Take(5)
            .Select(row => new SearchConsoleOpportunity
            {
                Query = row.Keys[0],
                Page = row.Keys[1],
                Clicks = row.Clicks,
                Impressions = row.Impressions,
                Ctr = row.Ctr,
                Position = row.Position
            })
            .ToList();
    }
}
