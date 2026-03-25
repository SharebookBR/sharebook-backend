using System;

namespace ShareBook.Api.ViewModels
{
    public class JobMonitorItemVM
    {
        public int Order { get; set; }
        public string JobName { get; set; }
        public string Description { get; set; }
        public string Interval { get; set; }
        public bool Active { get; set; }
        public string BestDayOfWeek { get; set; }
        public string BestTimeToExecute { get; set; }
        public DateTime? NextExecutionAt { get; set; }
        public DateTime? LastExecutionAt { get; set; }
        public double? LastExecutionDurationSeconds { get; set; }
        public string LastExecutionDetails { get; set; }
    }
}
