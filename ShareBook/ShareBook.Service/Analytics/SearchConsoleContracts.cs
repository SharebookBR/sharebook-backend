using System.Text.Json.Serialization;

namespace ShareBook.Service.Analytics;

public interface ISearchConsoleApiClient
{
    Task<SearchConsoleApiResponse> QueryAsync(
        SearchConsoleApiQuery query,
        CancellationToken cancellationToken = default);
}

public interface ISearchConsoleService
{
    Task<SearchConsoleAnalytics> GetOverviewAsync(
        CancellationToken cancellationToken = default);
}

public record SearchConsoleApiQuery(
    DateOnly StartDate,
    DateOnly EndDate,
    IReadOnlyList<string> Dimensions,
    int RowLimit = 10);

public class SearchConsoleApiResponse
{
    [JsonPropertyName("rows")]
    public List<SearchConsoleApiRow> Rows { get; set; } = [];
}

public class SearchConsoleApiRow
{
    [JsonPropertyName("keys")]
    public List<string> Keys { get; set; } = [];

    [JsonPropertyName("clicks")]
    public double Clicks { get; set; }

    [JsonPropertyName("impressions")]
    public double Impressions { get; set; }

    [JsonPropertyName("ctr")]
    public double Ctr { get; set; }

    [JsonPropertyName("position")]
    public double Position { get; set; }
}
