using ShareBook.VM.Book.Model;
using ShareBook.VM.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public interface IBookService
    {
        Task<List<BookVM>> GetBooks();
        Task<BookVM> GetBookById(int id);
        Task<ResultServiceVM> CreateBook(BookVM bookVM);
    }
}
