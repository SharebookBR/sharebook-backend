using System.Collections.Generic;

namespace ShareBook.Service.Analytics;

public class AnalyticsDashboardDto
{
    public List<WeeklyPoint> Sessions { get; set; } = [];
    public List<WeeklyPoint> Downloads { get; set; } = [];
    public int TotalDownloads { get; set; }
    public int TotalLogins { get; set; }
    public int TotalSignups { get; set; }
    public List<WeeklyPoint> Logins { get; set; } = [];
    public List<WeeklyPoint> Signups { get; set; } = [];
    public List<BookMetric> TopBooksByViews { get; set; } = [];
    public List<BookMetric> TopBooksByDownloads { get; set; } = [];
    public Dictionary<string, List<BookMetric>> TopBooksByViewsPerWeek { get; set; } = new();
    public Dictionary<string, List<BookMetric>> TopBooksByDownloadsPerWeek { get; set; } = new();
    public List<EventMetric> EventSummary { get; set; } = [];
    public Dictionary<string, List<EventMetric>> EventSummaryPerWeek { get; set; } = new();
    public SearchAnalytics SearchAnalytics { get; set; } = new();
    public SearchConsoleAnalytics SearchConsole { get; set; } = new();
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

public class EventMetric
{
    public string EventName { get; set; }
    public int Count { get; set; }
    public int Users { get; set; }
}

public class SearchAnalytics
{
    public int TotalSearches { get; set; }
    public int Users { get; set; }
    public int DistinctTerms { get; set; }
    public List<SearchTermMetric> TopTerms { get; set; } = [];
    public List<SearchDeviceMetric> Devices { get; set; } = [];
}

public class SearchTermMetric
{
    public string Term { get; set; }
    public int Count { get; set; }
    public int Users { get; set; }
}

public class SearchDeviceMetric
{
    public string Device { get; set; }
    public int Count { get; set; }
    public int Users { get; set; }
}

public class SearchConsoleAnalytics
{
    public bool Available { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
    public SearchConsoleMetricSummary Current { get; set; } = new();
    public SearchConsoleMetricSummary Previous { get; set; } = new();
    public List<SearchConsoleDailyMetric> Daily { get; set; } = [];
    public List<SearchConsoleOpportunity> Opportunities { get; set; } = [];
}

public class SearchConsoleMetricSummary
{
    public double Clicks { get; set; }
    public double Impressions { get; set; }
    public double Ctr { get; set; }
    public double Position { get; set; }
}

public class SearchConsoleDailyMetric
{
    public string Date { get; set; }
    public double Clicks { get; set; }
    public double Impressions { get; set; }
}

public class SearchConsoleOpportunity : SearchConsoleMetricSummary
{
    public string Query { get; set; }
    public string Page { get; set; }
}
