using ShareBook.Domain;
using System.Linq;
using System.Threading.Tasks;

namespace ShareBook.Repository;

public class BookDownloadEventRepository(ApplicationDbContext context) : IBookDownloadEventRepository
{
    private readonly EntityCrud<BookDownloadEvent> _crud = new EntityCrud<BookDownloadEvent>(context);

    public IQueryable<BookDownloadEvent> Get() => _crud.Get();

    public Task<BookDownloadEvent> InsertAsync(BookDownloadEvent entity) => _crud.InsertAsync(entity);
}
