using ShareBook.Data.Entities.Book;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareBook.Repository
{
    public interface IBookRepository : IRepositoryGeneric<Book>
    {
        Task<List<Book>> GetBooks();
        Task<Book> GetBookById(int id);
    }
}
