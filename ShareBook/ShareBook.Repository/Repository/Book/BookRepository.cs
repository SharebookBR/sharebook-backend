using ShareBook.Data;
using ShareBook.Data.Entities.Book.Model;
using ShareBook.Data.Entities.Book.Out;
using ShareBook.Data.Model;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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

        public async Task<BookOutById> GetBookById(int id)
        {
            BookOutById bookOutById = new BookOutById();

            bookOutById.Book = await _context.Books.Where(e => e.Id == id).Select(x => new BookModel
            {
                Id = x.Id,
                Name = x.Name,

            }).FirstOrDefaultAsync();

            return bookOutById;
        }

        public async Task<BookOut> GetBooks()
        {
            BookOut bookOut = new BookOut();

            bookOut.Books = await _context.Books.Select(x => new BookModel
            {
                Id = x.Id,
                Name = x.Name,

            }).ToListAsync();

            return bookOut;
        }
    }
}
