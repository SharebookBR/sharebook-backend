using ShareBook.Domain;
using System.Threading.Tasks;

namespace ShareBook.Repository
{
    public class JobHistoryReposiory : RepositoryGeneric<JobHistory>, IJobHistoryReposiory
    {
        public JobHistoryReposiory(ApplicationDbContext context) : base(context)
        {
        }
    }
}
