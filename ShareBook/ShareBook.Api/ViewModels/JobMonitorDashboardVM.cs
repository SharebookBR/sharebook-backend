using System.Collections.Generic;

namespace ShareBook.Api.ViewModels
{
    public class JobMonitorDashboardVM
    {
        public JobMonitorSummaryVM Summary { get; set; }
        public JobMonitorExecutorVM Executor { get; set; }
        public IList<JobMonitorItemVM> Jobs { get; set; }
    }
}
