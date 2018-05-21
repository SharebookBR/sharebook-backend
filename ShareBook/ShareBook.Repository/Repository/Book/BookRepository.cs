using ShareBook.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShareBook.Data.Entities.Book;
using System.Collections.Generic;

namespace ShareBook.Repository
{
    public class BookRepository : RepositoryGeneric<Book>, IBookRepository
    {
        private readonly ApplicationDbContext _context;

        public BookRepository(ApplicationDbContext context)
        : base(context)
        {
            _context = context;
        }

        public async Task<Book> GetBookById(int id)
        {
            Book book = new Book();

            book = await _context.Books.Where(e => e.Id == id).Select(x => new Book
            {
                Id = x.Id,
                Name = x.Name,

            }).FirstOrDefaultAsync();

            return book;
        }

        public IQueryable<Book> GetBooks()
        {
            return _context.Books;
        }
    }
}
