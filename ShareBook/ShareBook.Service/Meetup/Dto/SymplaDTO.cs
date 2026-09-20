using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace ShareBook.Service.Dto;

// 24/11/2024 - Paramos de integrar com sympla. Agora carregamos a lista de meetups apenas do YOUTUBE.
public class SymplaDto
{
    public string Status { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<SymplaEvent> Data { get; set; } = [];
}
public class SymplaEvent
{
    public int Id { get; set; }
    [JsonProperty("start_date")]
    public string StartDate { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
