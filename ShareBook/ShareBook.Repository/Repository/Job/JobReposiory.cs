using ShareBook.Domain;
using System.Threading.Tasks;

namespace ShareBook.Repository
{
    public class JobReposiory : RepositoryGeneric<Job>, IJobReposiory
    {
        public JobReposiory(ApplicationDbContext context) : base(context)
        {
        }
    }

    public class JobHistoryReposiory : RepositoryGeneric<JobHistory>, IJobHistoryReposiory
    {
        public JobHistoryReposiory(ApplicationDbContext context) : base(context)
        {
        }
    }
}
