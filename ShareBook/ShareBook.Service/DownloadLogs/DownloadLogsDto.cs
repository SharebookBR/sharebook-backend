using System;
using System.Collections.Generic;

namespace ShareBook.Service.DownloadLogs;

public class DownloadLogsSummaryDto
{
    public DateTime Day { get; set; }
    public int Allowed { get; set; }
    public int BlockedThrottle { get; set; }
    public int BlockedDailyLimit { get; set; }
}

public class DownloadLogEventDto
{
    public DateTime Timestamp { get; set; }
    public string Ip { get; set; } = string.Empty;
    public string Outcome { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

public class PagedDownloadLogEventsDto
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public IList<DownloadLogEventDto> Items { get; set; } = new List<DownloadLogEventDto>();
}
