using ShareBook.Domain;
using System.Linq;
using System.Threading.Tasks;

namespace ShareBook.Repository;

public interface IBookDownloadEventRepository
{
    IQueryable<BookDownloadEvent> Get();

    Task<BookDownloadEvent> InsertAsync(BookDownloadEvent entity);
}
