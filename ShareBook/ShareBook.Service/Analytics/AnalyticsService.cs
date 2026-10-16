using Google.Analytics.Data.V1Beta;
using Google.Apis.Auth.OAuth2;
using Grpc.Auth;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShareBook.Service.Analytics;

public class AnalyticsService(
    IOptions<GA4Settings> settings,
    IMemoryCache cache,
    ISearchConsoleService searchConsoleService,
    ILogger<AnalyticsService> logger) : IAnalyticsService
{
    private const string PropertyId = "386966473";
    private const string CacheKey = "ga4_dashboard";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);
    private static readonly TimeSpan StaleFallbackDuration = TimeSpan.FromHours(1);

    private readonly GA4Settings _settings = settings.Value;
    private readonly IMemoryCache _cache = cache;
    private readonly ISearchConsoleService _searchConsoleService = searchConsoleService;
    private readonly ILogger<AnalyticsService> _logger = logger;
    private readonly object _refreshGate = new();
    private Task<AnalyticsDashboardDto>? _refreshTask;
    private AnalyticsDashboardDto? _lastSuccessfulSnapshot;
    private DateTimeOffset? _lastSuccessfulRefreshAtUtc;

    public async Task<AnalyticsDashboardDto> GetDashboardAsync()
    {
        if (_cache.TryGetValue(CacheKey, out AnalyticsDashboardDto? cached) && cached != null)
        {
            _logger.LogInformation(
                "Analytics dashboard cache hit. CacheAgeSeconds={CacheAgeSeconds}",
                GetCacheAgeSeconds());
            return cached;
        }

        _logger.LogInformation("Analytics dashboard cache miss. Source={Source}", "request");
        return await RefreshAsync("request");
    }

    public async Task WarmCacheAsync(string source, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Analytics dashboard cache warm requested. Source={Source}", source);
        await RefreshAsync(source, force: true, cancellationToken);
    }

    private async Task<AnalyticsDashboardDto> RefreshAsync(
        string source,
        bool force = false,
        CancellationToken cancellationToken = default)
    {
        Task<AnalyticsDashboardDto> refreshTask;

        lock (_refreshGate)
        {
            if (_refreshTask is { IsCompleted: false })
            {
                _logger.LogInformation(
                    "Analytics dashboard refresh already in progress. Source={Source}",
                    source);
                refreshTask = _refreshTask;
            }
            else if (!force && _cache.TryGetValue(CacheKey, out AnalyticsDashboardDto? cached) && cached != null)
            {
                return cached;
            }
            else
            {
                _refreshTask = RefreshCoreAsync(source);
                refreshTask = _refreshTask;
            }
        }

        return await refreshTask.WaitAsync(cancellationToken);
    }

    private async Task<AnalyticsDashboardDto> RefreshCoreAsync(string source)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation(
            "Analytics dashboard refresh started. Source={Source}",
            source);

        try
        {
            var client = BuildClient();
            var dto = await FetchAsync(client);

            _cache.Set(CacheKey, dto, CacheDuration);
            _lastSuccessfulSnapshot = dto;
            _lastSuccessfulRefreshAtUtc = DateTimeOffset.UtcNow;

            _logger.LogInformation(
                "Analytics dashboard refresh succeeded. Source={Source} ElapsedMs={ElapsedMs} CacheDurationHours={CacheDurationHours}",
                source,
                stopwatch.Elapsed.TotalMilliseconds,
                CacheDuration.TotalHours);

            return dto;
        }
        catch (Exception exception)
        {
            if (_cache.TryGetValue(CacheKey, out AnalyticsDashboardDto? cached) && cached != null)
            {
                _logger.LogWarning(
                    exception,
                    "Analytics dashboard refresh failed; preserving current cache. Source={Source} ElapsedMs={ElapsedMs} SnapshotAgeSeconds={SnapshotAgeSeconds}",
                    source,
                    stopwatch.Elapsed.TotalMilliseconds,
                    GetCacheAgeSeconds());
                return cached;
            }

            if (_lastSuccessfulSnapshot != null)
            {
                _cache.Set(CacheKey, _lastSuccessfulSnapshot, StaleFallbackDuration);
                _logger.LogWarning(
                    exception,
                    "Analytics dashboard refresh failed; serving stale snapshot. Source={Source} ElapsedMs={ElapsedMs} SnapshotAgeSeconds={SnapshotAgeSeconds}",
                    source,
                    stopwatch.Elapsed.TotalMilliseconds,
                    GetCacheAgeSeconds());
                return _lastSuccessfulSnapshot;
            }

            _logger.LogError(
                exception,
                "Analytics dashboard refresh failed with no snapshot available. Source={Source} ElapsedMs={ElapsedMs}",
                source,
                stopwatch.Elapsed.TotalMilliseconds);
            throw;
        }
        finally
        {
            lock (_refreshGate)
            {
                _refreshTask = null;
            }
        }
    }

    private double? GetCacheAgeSeconds()
    {
        return _lastSuccessfulRefreshAtUtc.HasValue
            ? (DateTimeOffset.UtcNow - _lastSuccessfulRefreshAtUtc.Value).TotalSeconds
            : null;
    }

    private BetaAnalyticsDataClient BuildClient()
    {
        var json = Encoding.UTF8.GetString(Convert.FromBase64String(_settings.CredentialsBase64));
        var credential = CredentialFactory.FromJson<ServiceAccountCredential>(json)
            .ToGoogleCredential()
            .CreateScoped("https://www.googleapis.com/auth/analytics.readonly");

        return new BetaAnalyticsDataClientBuilder
        {
            ChannelCredentials = credential.ToChannelCredentials()
        }.Build();
    }

    private async Task<AnalyticsDashboardDto> FetchAsync(BetaAnalyticsDataClient client)
    {
        var searchConsoleTask = MeasureAsync("search_console", FetchSearchConsoleSafeAsync);
        var property = $"properties/{PropertyId}";
        var dateRange = new DateRange { StartDate = "84daysAgo", EndDate = "today" };

        var sessions = await MeasureAsync("ga4.sessions_by_week", () => FetchWeeklyMetricAsync(client, property, dateRange, "sessions"));
        var downloads = await MeasureAsync("ga4.downloads_by_week", () => FetchWeeklyEventAsync(client, property, dateRange, "ebook_download"));
        var totalDownloads = await MeasureAsync("ga4.total_downloads", () => FetchEventTotalAsync(client, property, dateRange, "ebook_download"));
        var totalLogins = await MeasureAsync("ga4.total_logins", () => FetchEventTotalAsync(client, property, dateRange, "login"));
        var totalSignups = await MeasureAsync("ga4.total_signups", () => FetchEventTotalAsync(client, property, dateRange, "sign_up"));
        var loginsByWeek = await MeasureAsync("ga4.logins_by_week", () => FetchWeeklyEventAsync(client, property, dateRange, "login"));
        var signupsByWeek = await MeasureAsync("ga4.signups_by_week", () => FetchWeeklyEventAsync(client, property, dateRange, "sign_up"));
        var topViews = await MeasureAsync("ga4.top_books_by_views", () => FetchTopBooksAsync(client, property, dateRange, byDownload: false));
        var topDownloads = await MeasureAsync("ga4.top_books_by_downloads", () => FetchTopBooksAsync(client, property, dateRange, byDownload: true));
        var topViewsPerWeek = await MeasureAsync("ga4.top_books_by_views_per_week", () => FetchTopBooksWeeklyAsync(client, property, dateRange, byDownload: false));
        var topDownloadsPerWeek = await MeasureAsync("ga4.top_books_by_downloads_per_week", () => FetchTopBooksWeeklyAsync(client, property, dateRange, byDownload: true));
        var eventSummary = await MeasureAsync("ga4.event_summary", () => FetchEventSummaryAsync(client, property, dateRange));
        var eventSummaryPerWeek = await MeasureAsync("ga4.event_summary_per_week", () => FetchEventSummaryWeeklyAsync(client, property, dateRange));
        var searchAnalytics = await MeasureAsync("ga4.search_analytics", () => FetchSearchAnalyticsAsync(client, property));

        return new AnalyticsDashboardDto
        {
            Sessions = sessions,
            Downloads = downloads,
            TotalDownloads = totalDownloads,
            TotalLogins = totalLogins,
            TotalSignups = totalSignups,
            Logins = loginsByWeek,
            Signups = signupsByWeek,
            TopBooksByViews = topViews,
            TopBooksByDownloads = topDownloads,
            TopBooksByViewsPerWeek = topViewsPerWeek,
            TopBooksByDownloadsPerWeek = topDownloadsPerWeek,
            EventSummary = eventSummary,
            EventSummaryPerWeek = eventSummaryPerWeek,
            SearchAnalytics = searchAnalytics,
            SearchConsole = await searchConsoleTask
        };
    }

    private async Task<T> MeasureAsync<T>(string operation, Func<Task<T>> action)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await action();
            _logger.LogInformation(
                "Analytics dashboard dependency completed. Operation={Operation} ElapsedMs={ElapsedMs}",
                operation,
                stopwatch.Elapsed.TotalMilliseconds);
            return result;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Analytics dashboard dependency failed. Operation={Operation} ElapsedMs={ElapsedMs}",
                operation,
                stopwatch.Elapsed.TotalMilliseconds);
            throw;
        }
    }

    private async Task<SearchConsoleAnalytics> FetchSearchConsoleSafeAsync()
    {
        try
        {
            return await _searchConsoleService.GetOverviewAsync();
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Search Console indisponivel; mantendo o dashboard com dados do GA4.");
            return new SearchConsoleAnalytics();
        }
    }

    private async Task<SearchAnalytics> FetchSearchAnalyticsAsync(
        BetaAnalyticsDataClient client, string property)
    {
        // Date ranges are inclusive in GA4: today + 29 previous days = 30 days.
        var dateRange = new DateRange { StartDate = "29daysAgo", EndDate = "today" };
        var eventFilter = new FilterExpression
        {
            Filter = new Filter
            {
                FieldName = "eventName",
                StringFilter = new Filter.Types.StringFilter
                {
                    Value = "search",
                    MatchType = Filter.Types.StringFilter.Types.MatchType.Exact
                }
            }
        };

        var totalsResponse = await client.RunReportAsync(new RunReportRequest
        {
            Property = property,
            DateRanges = { dateRange },
            Metrics = { new Metric { Name = "eventCount" }, new Metric { Name = "totalUsers" } },
            DimensionFilter = eventFilter
        });

        var termsResponse = await client.RunReportAsync(new RunReportRequest
        {
            Property = property,
            DateRanges = { dateRange },
            Dimensions = { new Dimension { Name = "customEvent:search_term" } },
            Metrics = { new Metric { Name = "eventCount" }, new Metric { Name = "totalUsers" } },
            DimensionFilter = eventFilter,
            OrderBys =
            {
                new OrderBy
                {
                    Metric = new OrderBy.Types.MetricOrderBy { MetricName = "eventCount" },
                    Desc = true
                }
            },
            Limit = 10
        });

        var devicesResponse = await client.RunReportAsync(new RunReportRequest
        {
            Property = property,
            DateRanges = { dateRange },
            Dimensions = { new Dimension { Name = "deviceCategory" } },
            Metrics = { new Metric { Name = "eventCount" }, new Metric { Name = "totalUsers" } },
            DimensionFilter = eventFilter,
            OrderBys =
            {
                new OrderBy
                {
                    Metric = new OrderBy.Types.MetricOrderBy { MetricName = "eventCount" },
                    Desc = true
                }
            }
        });

        var totals = totalsResponse.Rows.FirstOrDefault();

        return new SearchAnalytics
        {
            TotalSearches = totals is null ? 0 : int.Parse(totals.MetricValues[0].Value),
            Users = totals is null ? 0 : int.Parse(totals.MetricValues[1].Value),
            DistinctTerms = termsResponse.RowCount,
            TopTerms = termsResponse.Rows.Select(row => new SearchTermMetric
            {
                Term = row.DimensionValues[0].Value,
                Count = int.Parse(row.MetricValues[0].Value),
                Users = int.Parse(row.MetricValues[1].Value)
            }).ToList(),
            Devices = devicesResponse.Rows.Select(row => new SearchDeviceMetric
            {
                Device = row.DimensionValues[0].Value,
                Count = int.Parse(row.MetricValues[0].Value),
                Users = int.Parse(row.MetricValues[1].Value)
            }).ToList()
        };
    }

    private async Task<List<WeeklyPoint>> FetchWeeklyMetricAsync(
        BetaAnalyticsDataClient client, string property, DateRange dateRange, string metric)
    {
        var response = await client.RunReportAsync(new RunReportRequest
        {
            Property = property,
            DateRanges = { dateRange },
            Dimensions = { new Dimension { Name = "year" }, new Dimension { Name = "week" } },
            Metrics = { new Metric { Name = metric } },
            OrderBys =
            {
                new OrderBy { Dimension = new OrderBy.Types.DimensionOrderBy { DimensionName = "year" } },
                new OrderBy { Dimension = new OrderBy.Types.DimensionOrderBy { DimensionName = "week" } }
            }
        });

        return response.Rows.Select(row => new WeeklyPoint
        {
            Label = $"{row.DimensionValues[0].Value}-W{row.DimensionValues[1].Value.PadLeft(2, '0')}",
            Value = int.Parse(row.MetricValues[0].Value)
        }).ToList();
    }

    private async Task<List<WeeklyPoint>> FetchWeeklyEventAsync(
        BetaAnalyticsDataClient client, string property, DateRange dateRange, string eventName)
    {
        var response = await client.RunReportAsync(new RunReportRequest
        {
            Property = property,
            DateRanges = { dateRange },
            Dimensions = { new Dimension { Name = "year" }, new Dimension { Name = "week" } },
            Metrics = { new Metric { Name = "eventCount" } },
            DimensionFilter = new FilterExpression
            {
                Filter = new Filter
                {
                    FieldName = "eventName",
                    StringFilter = new Filter.Types.StringFilter
                    {
                        Value = eventName,
                        MatchType = Filter.Types.StringFilter.Types.MatchType.Exact
                    }
                }
            },
            OrderBys =
            {
                new OrderBy { Dimension = new OrderBy.Types.DimensionOrderBy { DimensionName = "year" } },
                new OrderBy { Dimension = new OrderBy.Types.DimensionOrderBy { DimensionName = "week" } }
            }
        });

        return response.Rows.Select(row => new WeeklyPoint
        {
            Label = $"{row.DimensionValues[0].Value}-W{row.DimensionValues[1].Value.PadLeft(2, '0')}",
            Value = int.Parse(row.MetricValues[0].Value)
        }).ToList();
    }

    private async Task<int> FetchEventTotalAsync(
        BetaAnalyticsDataClient client, string property, DateRange dateRange, string eventName)
    {
        var response = await client.RunReportAsync(new RunReportRequest
        {
            Property = property,
            DateRanges = { dateRange },
            Dimensions = { new Dimension { Name = "eventName" } },
            Metrics = { new Metric { Name = "eventCount" } },
            DimensionFilter = new FilterExpression
            {
                Filter = new Filter
                {
                    FieldName = "eventName",
                    StringFilter = new Filter.Types.StringFilter
                    {
                        Value = eventName,
                        MatchType = Filter.Types.StringFilter.Types.MatchType.Exact
                    }
                }
            }
        });

        return response.Rows.FirstOrDefault() is { } row
            ? int.Parse(row.MetricValues[0].Value)
            : 0;
    }

    private async Task<List<BookMetric>> FetchTopBooksAsync(
        BetaAnalyticsDataClient client, string property, DateRange dateRange, bool byDownload)
    {
        var request = new RunReportRequest
        {
            Property = property,
            DateRanges = { dateRange },
            Dimensions = { new Dimension { Name = "pagePath" } },
            Metrics = { new Metric { Name = byDownload ? "eventCount" : "screenPageViews" } },
            DimensionFilter = new FilterExpression
            {
                Filter = new Filter
                {
                    FieldName = byDownload ? "eventName" : "pagePath",
                    StringFilter = new Filter.Types.StringFilter
                    {
                        Value = byDownload ? "ebook_download" : "/livros/",
                        MatchType = byDownload
                            ? Filter.Types.StringFilter.Types.MatchType.Exact
                            : Filter.Types.StringFilter.Types.MatchType.BeginsWith
                    }
                }
            },
            OrderBys =
            {
                new OrderBy
                {
                    Metric = new OrderBy.Types.MetricOrderBy { MetricName = byDownload ? "eventCount" : "screenPageViews" },
                    Desc = true
                }
            },
            Limit = 10
        };

        var response = await client.RunReportAsync(request);

        return response.Rows.Select(row =>
        {
            var path = row.DimensionValues[0].Value;
            var slug = path.TrimEnd('/').Split('/').Last();
            var title = string.IsNullOrEmpty(slug)
                ? path
                : System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(slug.Replace("-", " "));
            return new BookMetric
            {
                Path = path,
                Title = title,
                Count = int.Parse(row.MetricValues[0].Value)
            };
        }).ToList();
    }

    private async Task<Dictionary<string, List<BookMetric>>> FetchTopBooksWeeklyAsync(
        BetaAnalyticsDataClient client, string property, DateRange dateRange, bool byDownload)
    {
        var request = new RunReportRequest
        {
            Property = property,
            DateRanges = { dateRange },
            Dimensions =
            {
                new Dimension { Name = "year" },
                new Dimension { Name = "week" },
                new Dimension { Name = "pagePath" }
            },
            Metrics = { new Metric { Name = byDownload ? "eventCount" : "screenPageViews" } },
            DimensionFilter = new FilterExpression
            {
                Filter = new Filter
                {
                    FieldName = byDownload ? "eventName" : "pagePath",
                    StringFilter = new Filter.Types.StringFilter
                    {
                        Value = byDownload ? "ebook_download" : "/livros/",
                        MatchType = byDownload
                            ? Filter.Types.StringFilter.Types.MatchType.Exact
                            : Filter.Types.StringFilter.Types.MatchType.BeginsWith
                    }
                }
            },
            Limit = 500
        };

        var response = await client.RunReportAsync(request);

        var grouped = new Dictionary<string, List<BookMetric>>();

        foreach (var row in response.Rows)
        {
            var year = row.DimensionValues[0].Value;
            var week = row.DimensionValues[1].Value.PadLeft(2, '0');
            var weekKey = $"{year}-W{week}";
            var path = row.DimensionValues[2].Value;
            var slug = path.TrimEnd('/').Split('/').Last();
            var title = string.IsNullOrEmpty(slug)
                ? path
                : System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(slug.Replace("-", " "));
            var count = int.Parse(row.MetricValues[0].Value);

            if (!grouped.ContainsKey(weekKey))
                grouped[weekKey] = new List<BookMetric>();

            grouped[weekKey].Add(new BookMetric { Path = path, Title = title, Count = count });
        }

        foreach (var key in grouped.Keys.ToList())
            grouped[key] = grouped[key].OrderByDescending(b => b.Count).Take(10).ToList();

        return grouped;
    }

    private static readonly List<string> TrackedEvents =
    [
        "ebook_download", "amazon_click", "share_modal_open",
        "social_share", "search", "login", "sign_up",
        "book_request_modal_open", "book_request_success", "book_request_error"
    ];

    private async Task<List<EventMetric>> FetchEventSummaryAsync(
        BetaAnalyticsDataClient client, string property, DateRange dateRange)
    {
        var response = await client.RunReportAsync(new RunReportRequest
        {
            Property = property,
            DateRanges = { dateRange },
            Dimensions = { new Dimension { Name = "eventName" } },
            Metrics = { new Metric { Name = "eventCount" }, new Metric { Name = "totalUsers" } },
            DimensionFilter = new FilterExpression
            {
                Filter = new Filter
                {
                    FieldName = "eventName",
                    InListFilter = new Filter.Types.InListFilter { Values = { TrackedEvents } }
                }
            }
        });

        var dict = response.Rows.ToDictionary(
            r => r.DimensionValues[0].Value,
            r => new EventMetric
            {
                EventName = r.DimensionValues[0].Value,
                Count = int.Parse(r.MetricValues[0].Value),
                Users = int.Parse(r.MetricValues[1].Value)
            });

        return TrackedEvents.Select(e => dict.TryGetValue(e, out var m) ? m : new EventMetric { EventName = e }).ToList();
    }

    private async Task<Dictionary<string, List<EventMetric>>> FetchEventSummaryWeeklyAsync(
        BetaAnalyticsDataClient client, string property, DateRange dateRange)
    {
        var response = await client.RunReportAsync(new RunReportRequest
        {
            Property = property,
            DateRanges = { dateRange },
            Dimensions =
            {
                new Dimension { Name = "year" },
                new Dimension { Name = "week" },
                new Dimension { Name = "eventName" }
            },
            Metrics = { new Metric { Name = "eventCount" }, new Metric { Name = "totalUsers" } },
            DimensionFilter = new FilterExpression
            {
                Filter = new Filter
                {
                    FieldName = "eventName",
                    InListFilter = new Filter.Types.InListFilter { Values = { TrackedEvents } }
                }
            }
        });

        var grouped = new Dictionary<string, Dictionary<string, EventMetric>>();

        foreach (var row in response.Rows)
        {
            var weekKey = $"{row.DimensionValues[0].Value}-W{row.DimensionValues[1].Value.PadLeft(2, '0')}";
            var eventName = row.DimensionValues[2].Value;

            if (!grouped.ContainsKey(weekKey))
                grouped[weekKey] = new Dictionary<string, EventMetric>();

            grouped[weekKey][eventName] = new EventMetric
            {
                EventName = eventName,
                Count = int.Parse(row.MetricValues[0].Value),
                Users = int.Parse(row.MetricValues[1].Value)
            };
        }

        return grouped.ToDictionary(
            kv => kv.Key,
            kv => TrackedEvents.Select(e => kv.Value.TryGetValue(e, out var m) ? m : new EventMetric { EventName = e }).ToList()
        );
    }
}
