using System;
using System.Collections.Generic;

namespace ShareBook.Service.Dto;

public class YoutubeDto
{
    public string nextPageToken { get; set; } = string.Empty;
    public string prevPageToken { get; set; } = string.Empty;
    public PageInfo pageInfo { get; set; } = null!;
    public List<Item> Items { get; set; } = [];
}
public class PageInfo
{
    public int TotalResults { get; set; }
    public int ResultsPerPage { get; set; }
}

public class Item
{
    public Id Id { get; set; } = null!;
    public Snippet Snippet { get; set; } = null!;
}

public class Id
{
    public string Kind { get; set; } = string.Empty;
    public string VideoId { get; set; } = string.Empty;
}

public class Snippet
{
    public DateTime PublishedAt { get; set; }
    public string ChannelId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ThumbnailsDto Thumbnails { get; set; } = null!;
    public string ChannelTitle { get; set; } = string.Empty;
    public string LiveBroadcastContent { get; set; } = string.Empty;
    public DateTime PublishTime { get; set; }
}

public class ThumbnailsDto
{
    public ThumbnailDetailDto Default { get; set; } = null!;
    public ThumbnailDetailDto Medium { get; set; } = null!;
    public ThumbnailDetailDto High { get; set; } = null!;
}

public class ThumbnailDetailDto
{
    public string Url { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
}

