using ShareBook.Domain.Enums;
using System;

namespace Sharebook.Jobs
{
    public interface IJob
    {
        string JobName { get; set; }
        string Description { get; set; }
        Interval Interval { get; set; }
        bool Active { get; set; }
        TimeSpan? BestTimeToExecute { get; set; }

        bool HasWork();
        bool Execute();
    }
}
