using System.Collections.Generic;
using System.Threading.Tasks;
using ShareBook.VM.Book.Model;
using ShareBook.VM.Common;

namespace ShareBook.Service
{
    public interface IBookService
    {
        Task<ResultServiceVM> CreateBookAsync(BookVM bookVM);

        Task<BookVM> GetBookByIdAsync(int id);

        Task<List<BookVM>> GetBooksAsync();
    }
}