using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using ShareBook.Service.Analytics;
using Xunit;

namespace ShareBook.Test.Unit.Services;

public class SearchConsoleServiceTests
{
    [Fact]
    public async Task GetOverviewAsync_builds_comparison_daily_series_and_ranked_opportunities()
    {
        var apiClient = new Mock<ISearchConsoleApiClient>();
        var currentStart = new DateOnly(2026, 7, 27);
        var currentEnd = new DateOnly(2026, 8, 23);
        var previousStart = new DateOnly(2026, 6, 29);
        var previousEnd = new DateOnly(2026, 7, 26);

        apiClient
            .Setup(client => client.QueryAsync(
                It.Is<SearchConsoleApiQuery>(query =>
                    query.StartDate == currentStart && query.Dimensions.Count == 0),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response(Row(clicks: 120, impressions: 2_000, ctr: 0.06, position: 8)));

        apiClient
            .Setup(client => client.QueryAsync(
                It.Is<SearchConsoleApiQuery>(query =>
                    query.StartDate == previousStart && query.Dimensions.Count == 0),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response(Row(clicks: 90, impressions: 1_800, ctr: 0.05, position: 6)));

        apiClient
            .Setup(client => client.QueryAsync(
                It.Is<SearchConsoleApiQuery>(query =>
                    query.Dimensions.SequenceEqual(new[] { "date" })),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response(
                Row(["2026-08-23"], clicks: 7, impressions: 100),
                Row(["invalid"], clicks: 99, impressions: 999),
                Row(["2026-08-22"], clicks: 5, impressions: 80)));

        apiClient
            .Setup(client => client.QueryAsync(
                It.Is<SearchConsoleApiQuery>(query =>
                    query.Dimensions.SequenceEqual(new[] { "query", "page" })),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response(
                Row(["primeira", "https://www.sharebook.com.br/livros/primeira"], impressions: 200, ctr: 0.01, position: 8),
                Row(["segunda", "https://www.sharebook.com.br/livros/segunda"], impressions: 100, ctr: 0, position: 4),
                Row(["ctr alto", "https://www.sharebook.com.br/livros/fora"], impressions: 500, ctr: 0.08, position: 3),
                Row(["poucas impressoes", "https://www.sharebook.com.br/livros/fora"], impressions: 19, ctr: 0, position: 3),
                Row(["posicao distante", "https://www.sharebook.com.br/livros/fora"], impressions: 500, ctr: 0, position: 21)));

        var service = new SearchConsoleService(
            apiClient.Object,
            new FixedTimeProvider(new DateTimeOffset(2026, 8, 26, 12, 0, 0, TimeSpan.Zero)));

        var result = await service.GetOverviewAsync();

        Assert.True(result.Available);
        Assert.Equal("2026-07-27", result.StartDate);
        Assert.Equal("2026-08-23", result.EndDate);
        Assert.Equal(120, result.Current.Clicks);
        Assert.Equal(90, result.Previous.Clicks);
        Assert.Equal(previousEnd, currentStart.AddDays(-1));
        Assert.Equal(2, result.Daily.Count);
        Assert.Equal("2026-08-22", result.Daily[0].Date);
        Assert.Collection(
            result.Opportunities,
            opportunity => Assert.Equal("primeira", opportunity.Query),
            opportunity => Assert.Equal("segunda", opportunity.Query));
        apiClient.Verify(
            client => client.QueryAsync(
                It.Is<SearchConsoleApiQuery>(query =>
                    query.StartDate == previousStart && query.EndDate == previousEnd),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetOverviewAsync_returns_available_zero_summary_when_property_has_no_rows()
    {
        var apiClient = new Mock<ISearchConsoleApiClient>();
        apiClient
            .Setup(client => client.QueryAsync(
                It.IsAny<SearchConsoleApiQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SearchConsoleApiResponse());

        var service = new SearchConsoleService(
            apiClient.Object,
            new FixedTimeProvider(new DateTimeOffset(2026, 8, 26, 12, 0, 0, TimeSpan.Zero)));

        var result = await service.GetOverviewAsync();

        Assert.True(result.Available);
        Assert.Equal(0, result.Current.Clicks);
        Assert.Empty(result.Daily);
        Assert.Empty(result.Opportunities);
    }

    private static SearchConsoleApiResponse Response(params SearchConsoleApiRow[] rows)
        => new() { Rows = rows.ToList() };

    private static SearchConsoleApiRow Row(
        List<string> keys = null,
        double clicks = 0,
        double impressions = 0,
        double ctr = 0,
        double position = 0)
        => new()
        {
            Keys = keys ?? [],
            Clicks = clicks,
            Impressions = impressions,
            Ctr = ctr,
            Position = position
        };

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
