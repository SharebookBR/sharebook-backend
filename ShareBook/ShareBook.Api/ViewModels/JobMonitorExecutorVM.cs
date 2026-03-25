using System;

namespace ShareBook.Api.ViewModels
{
    public class JobMonitorExecutorVM
    {
        public DateTime? LastExecutionAt { get; set; }
        public bool? LastExecutionSuccess { get; set; }
        public double? LastExecutionDurationSeconds { get; set; }
        public string Details { get; set; }
    }
}
