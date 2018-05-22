using ShareBook.Data;
using ShareBook.Data.Entities.Book;

namespace ShareBook.Repository
{
    public class BookRepository : GenericRepository<Book>, IBookRepository
    {
        public BookRepository(ApplicationDbContext context) : base(context) { }
    }
}