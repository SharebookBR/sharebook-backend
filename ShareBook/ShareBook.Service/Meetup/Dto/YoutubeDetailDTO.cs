using System;
using System.Collections.Generic;

namespace ShareBook.Service.Dto;

public class YoutubeDtoDetail
{
    public string nextPageToken { get; set; } = string.Empty;
    public string prevPageToken { get; set; } = string.Empty;
    public PageInfo pageInfo { get; set; } = null!;
    public List<ItemDetail> Items { get; set; } = [];
    
}

public class ItemDetail
{
    public String Id { get; set; } = string.Empty;
    public Snippet Snippet { get; set; } = null!;
    public liveStreamingDetails liveStreamingDetails { get; set; } = null!;
}

public class liveStreamingDetails
{
    public DateTime scheduledStartTime { get; set; }
    public string activeLiveChatId { get; set; } = string.Empty;
}
