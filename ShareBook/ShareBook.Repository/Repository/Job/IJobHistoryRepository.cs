using ShareBook.Domain;
using System.Linq;
using System.Threading.Tasks;

namespace ShareBook.Repository;

public interface IJobHistoryRepository
{
    IQueryable<JobHistory> Get();

    Task<JobHistory> InsertAsync(JobHistory entity);
}
