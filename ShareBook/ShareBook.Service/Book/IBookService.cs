using ShareBook.VM.Book.Model;
using ShareBook.VM.Common;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public interface IBookService
    {
        Task<BookVM> GetBooks();
        Task<BookVM> GetBookById(int id);
        Task<ResultServiceVM> CreateBook(BookVM bookVM);
    }
}
