namespace ShareBook.Api.ViewModels;

public class JobMonitorSummaryVM
{
    public int TotalJobs { get; set; }
    public int ActiveJobs { get; set; }
    public int InactiveJobs { get; set; }
    public int JobsWithHistory { get; set; }
    public int JobsNeverExecuted { get; set; }
    public int HealthyJobs { get; set; }
    public int DelayedJobs { get; set; }
    public int JobsWithError { get; set; }
}
