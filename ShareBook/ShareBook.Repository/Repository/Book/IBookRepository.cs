using ShareBook.Data.Entities.Book;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShareBook.Repository
{
    public interface IBookRepository : IRepositoryGeneric<Book>
    {
        IQueryable<Book> GetBooks();
        Task<Book> GetBookById(int id);
    }
}
