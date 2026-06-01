using System.Collections.Generic;

namespace ShareBook.Service.Analytics;

public class AnalyticsDashboardDto
{
    public List<WeeklyPoint> Sessions { get; set; } = [];
    public List<WeeklyPoint> Downloads { get; set; } = [];
    public int TotalDownloads { get; set; }
    public int TotalLogins { get; set; }
    public int TotalSignups { get; set; }
    public List<BookMetric> TopBooksByViews { get; set; } = [];
    public List<BookMetric> TopBooksByDownloads { get; set; } = [];
}

public class WeeklyPoint
{
    public string Label { get; set; }
    public int Value { get; set; }
}

public class BookMetric
{
    public string Path { get; set; }
    public string Title { get; set; }
    public int Count { get; set; }
}
