using System.Collections.Generic;

namespace ShareBook.Api.ViewModels;

public class JobMonitorDashboardVM
{
    public JobMonitorSummaryVM Summary { get; set; } = new();
    public JobMonitorExecutorVM Executor { get; set; } = new();
    public IList<JobMonitorItemVM> Jobs { get; set; } = new List<JobMonitorItemVM>();
}
