using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Analytics.Data.V1Beta;
using Google.Apis.Auth.OAuth2;
using Grpc.Auth;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Text;

namespace ShareBook.Service.Analytics;

public class AnalyticsService : IAnalyticsService
{
    private const string PropertyId = "386966473";
    private const string CacheKey = "ga4_dashboard";

    private readonly GA4Settings _settings;
    private readonly IMemoryCache _cache;

    public AnalyticsService(IOptions<GA4Settings> settings, IMemoryCache cache)
    {
        _settings = settings.Value;
        _cache = cache;
    }

    public async Task<AnalyticsDashboardDto> GetDashboardAsync()
    {
        if (_cache.TryGetValue(CacheKey, out AnalyticsDashboardDto cached))
            return cached;

        var client = BuildClient();
        var dto = await FetchAsync(client);

        _cache.Set(CacheKey, dto, TimeSpan.FromHours(24));
        return dto;
    }

    private BetaAnalyticsDataClient BuildClient()
    {
        var json = Encoding.UTF8.GetString(Convert.FromBase64String(_settings.CredentialsBase64));
        var credential = GoogleCredential.FromJson(json)
            .CreateScoped("https://www.googleapis.com/auth/analytics.readonly");

        return new BetaAnalyticsDataClientBuilder
        {
            ChannelCredentials = credential.ToChannelCredentials()
        }.Build();
    }

    private async Task<AnalyticsDashboardDto> FetchAsync(BetaAnalyticsDataClient client)
    {
        var property = $"properties/{PropertyId}";
        var dateRange = new DateRange { StartDate = "84daysAgo", EndDate = "today" };

        var sessions = await FetchWeeklyMetricAsync(client, property, dateRange, "sessions");
        var downloads = await FetchWeeklyEventAsync(client, property, dateRange, "ebook_download");
        var totalDownloads = await FetchEventTotalAsync(client, property, dateRange, "ebook_download");
        var totalLogins = await FetchEventTotalAsync(client, property, dateRange, "login");
        var totalSignups = await FetchEventTotalAsync(client, property, dateRange, "sign_up");
        var topViews = await FetchTopBooksAsync(client, property, dateRange, byDownload: false);
        var topDownloads = await FetchTopBooksAsync(client, property, dateRange, byDownload: true);

        return new AnalyticsDashboardDto
        {
            Sessions = sessions,
            Downloads = downloads,
            TotalDownloads = totalDownloads,
            TotalLogins = totalLogins,
            TotalSignups = totalSignups,
            TopBooksByViews = topViews,
            TopBooksByDownloads = topDownloads
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
}
