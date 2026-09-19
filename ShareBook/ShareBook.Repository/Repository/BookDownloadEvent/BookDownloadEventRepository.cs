using ShareBook.Domain;

namespace ShareBook.Repository
{
    public class BookDownloadEventRepository : RepositoryGeneric<BookDownloadEvent>, IBookDownloadEventRepository
    {
        public BookDownloadEventRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
