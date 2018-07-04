using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Service.Generic;
using System;
using System.Collections.Generic;


namespace ShareBook.Service
{
    public interface IBookService : IBaseService<Book>
    {
        Result<Book> Approve(Guid bookId);

        IList<dynamic> GetAllFreightOptions();

        IList<Book> GetTop15NewBooks(int page);

        IList<Book> GetByTitle(string title);

        IList<Book> GetByAuthor(string author);

        PagedList<Book> GetAll(int page, int items);
    }
}
