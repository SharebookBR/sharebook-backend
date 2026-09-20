using System.Collections.Generic;
using System.Text.Json;

namespace ShareBook.Api.ViewModels;

public class StalwartWebhookVM
{
    public IList<StalwartWebhookEventVM> Events { get; set; } = new List<StalwartWebhookEventVM>();
}

public class StalwartWebhookEventVM
{
    public string Id { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public JsonElement Data { get; set; }
}
