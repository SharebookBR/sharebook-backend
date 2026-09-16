using System.Collections.Generic;
using System.Text.Json;

namespace ShareBook.Api.ViewModels;

public class StalwartWebhookVM
{
    public IList<StalwartWebhookEventVM> Events { get; set; } = new List<StalwartWebhookEventVM>();
}

public class StalwartWebhookEventVM
{
    public string Id { get; set; }
    public string CreatedAt { get; set; }
    public string Type { get; set; }
    public JsonElement Data { get; set; }
}
