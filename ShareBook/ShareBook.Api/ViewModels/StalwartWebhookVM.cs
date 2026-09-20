using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShareBook.Api.ViewModels;

public class StalwartWebhookVM
{
    [JsonPropertyName("events")]
    public IList<StalwartWebhookEventVM> Events { get; set; } = new List<StalwartWebhookEventVM>();
}

public class StalwartWebhookEventVM
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public JsonElement Data { get; set; }
}
