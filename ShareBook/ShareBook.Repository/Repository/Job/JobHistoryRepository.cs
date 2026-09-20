using ShareBook.Domain;
using System.Linq;
using System.Threading.Tasks;

namespace ShareBook.Repository;

public class JobHistoryRepository : IJobHistoryRepository
{
    private readonly EntityCrud<JobHistory> _crud;

    public JobHistoryRepository(ApplicationDbContext context)
    {
        _crud = new EntityCrud<JobHistory>(context);
    }

    public IQueryable<JobHistory> Get() => _crud.Get();

    public Task<JobHistory> InsertAsync(JobHistory entity) => _crud.InsertAsync(entity);
}
