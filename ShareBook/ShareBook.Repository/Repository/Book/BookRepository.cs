using ShareBook.Domain;

namespace ShareBook.Repository
{
    public class BookRepository : RepositoryGeneric<Book>, IBookRepository
    {
        public BookRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
