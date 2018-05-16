using ShareBook.VM.Book.In;
using ShareBook.VM.Book.Out;
using ShareBook.VM.Common;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public interface IBookService
    {
        Task<BookOutVM> GetBooks();
        Task<BookOutByIdVM> GetBookById(int id);
        Task<ResultServiceVM> CreateBook(BookInVM bookInVM);
    }
}
