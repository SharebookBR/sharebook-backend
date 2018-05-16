using ShareBook.Data.Entities.Book.Out;
using ShareBook.Data.Model;
using System.Threading.Tasks;

namespace ShareBook.Repository
{
    public interface IBookRepository : IRepositoryGeneric<Book>
    {
        Task<BookOut> GetBooks();
        Task<BookOutById> GetBookById(int id);
    }
}
