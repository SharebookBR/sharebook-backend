using ShareBook.Domain;
using System.Linq;
using System.Threading.Tasks;

namespace ShareBook.Repository;

public class BookDownloadEventRepository : IBookDownloadEventRepository
{
    private readonly EntityCrud<BookDownloadEvent> _crud;

    public BookDownloadEventRepository(ApplicationDbContext context)
    {
        _crud = new EntityCrud<BookDownloadEvent>(context);
    }

    public IQueryable<BookDownloadEvent> Get() => _crud.Get();

    public Task<BookDownloadEvent> InsertAsync(BookDownloadEvent entity) => _crud.InsertAsync(entity);
}
