using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;

namespace ShareBook.Service.Analytics;

public class SearchConsoleApiClient : ISearchConsoleApiClient
{
    private const string Property = "sc-domain:sharebook.com.br";
    private const string ReadonlyScope = "https://www.googleapis.com/auth/webmasters.readonly";

    private readonly ITokenAccess _tokenAccess;
    private readonly HttpClient _httpClient;

    public SearchConsoleApiClient(
        IOptions<GA4Settings> settings,
        HttpClient httpClient)
    {
        var json = Encoding.UTF8.GetString(
            Convert.FromBase64String(settings.Value.CredentialsBase64));

        var credential = CredentialFactory.FromJson<ServiceAccountCredential>(json)
            .ToGoogleCredential()
            .CreateScoped(ReadonlyScope);
        _tokenAccess = (ITokenAccess)credential.UnderlyingCredential;
        _httpClient = httpClient;
    }

    public async Task<SearchConsoleApiResponse> QueryAsync(
        SearchConsoleApiQuery query,
        CancellationToken cancellationToken = default)
    {
        var accessToken = await _tokenAccess.GetAccessTokenForRequestAsync(
            null,
            cancellationToken);

        var payload = new Dictionary<string, object>
        {
            ["startDate"] = query.StartDate.ToString("yyyy-MM-dd"),
            ["endDate"] = query.EndDate.ToString("yyyy-MM-dd"),
            ["rowLimit"] = query.RowLimit
        };

        if (query.Dimensions.Count > 0)
            payload["dimensions"] = query.Dimensions;

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"https://www.googleapis.com/webmasters/v3/sites/{Uri.EscapeDataString(Property)}/searchAnalytics/query")
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<SearchConsoleApiResponse>(
            cancellationToken: cancellationToken) ?? new SearchConsoleApiResponse();
    }
}
