using ShareBook.Domain;
using ShareBook.Repository.Repository;

namespace ShareBook.Repository
{
    public class BookDownloadRepository : RepositoryGeneric<BookDownload>, IBookDownloadRepository
    {
        public BookDownloadRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}