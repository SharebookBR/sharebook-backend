using System;
using System.Collections.Generic;

namespace ShareBook.Service.Dto;

public class YoutubeDtoDetail
{
    public string nextPageToken { get; set; }
    public string prevPageToken { get; set; }
    public PageInfo pageInfo { get; set; }
    public List<ItemDetail> Items { get; set; }
    
}

public class ItemDetail
{
    public String Id { get; set; }
    public Snippet Snippet { get; set; }
    public liveStreamingDetails liveStreamingDetails { get; set; }
}

public class liveStreamingDetails
{
    public DateTime scheduledStartTime { get; set; }
    public string activeLiveChatId { get; set; }
}
