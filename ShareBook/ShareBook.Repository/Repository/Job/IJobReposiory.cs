using ShareBook.Domain;

namespace ShareBook.Repository
{
    public interface IJobReposiory : IRepositoryGeneric<Job>
    {
    }

    public interface IJobHistoryReposiory : IRepositoryGeneric<JobHistory>
    {
    }
}