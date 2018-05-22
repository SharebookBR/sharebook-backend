using System.Collections.Generic;
using ShareBook.VM.Book.Model;
using ShareBook.VM.Common;

namespace ShareBook.Service
{
    public interface IBookService
    {
        IEnumerable<BookVM> GetAll();

        BookVM GetById(int id);

        ResultServiceVM Create(BookVM bookVM);
    }
}