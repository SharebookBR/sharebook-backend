using System.Collections.Generic;
using System.Threading.Tasks;
using ShareBook.Data.Entities.Book;

namespace ShareBook.Repository
{
    public interface IBookRepository : IRepositoryGeneric<Book>
    {
        Task<Book> GetBookByIdAsync(int id);

        IEnumerable<Book> GetBooks();
    }
}