using ShareBook.Domain.Enums;
using System;
using System.Threading.Tasks;

namespace Sharebook.Jobs
{
    public interface IJob
    {
        string JobName { get; set; }
        string Description { get; set; }
        Interval Interval { get; set; }
        bool Active { get; set; }
        DayOfWeek? BestDayOfWeek { get; set; }
        TimeSpan? BestTimeToExecute { get; set; }

        bool HasWork();
        DateTime? GetNextExecutionAtUtc();
        Task<JobResult> ExecuteAsync();
    }
}
