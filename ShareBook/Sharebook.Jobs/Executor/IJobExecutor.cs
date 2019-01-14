using System;
using ShareBook.Domain.Common;

namespace Sharebook.Jobs
{
    public interface IJobExecutor
    {
        JobExecutorResult Execute();
    }
}
