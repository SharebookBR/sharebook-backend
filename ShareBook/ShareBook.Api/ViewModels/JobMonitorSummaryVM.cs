namespace ShareBook.Api.ViewModels
{
    public class JobMonitorSummaryVM
    {
        public int TotalJobs { get; set; }
        public int ActiveJobs { get; set; }
        public int InactiveJobs { get; set; }
        public int JobsWithHistory { get; set; }
        public int JobsNeverExecuted { get; set; }
    }
}
