using ShareBook.Domain.Common;
using System.Threading.Tasks;

namespace Sharebook.Jobs
{
    public interface IJobExecutor
    {
        Task<JobExecutorResult> ExecuteAsync();
    }
}
