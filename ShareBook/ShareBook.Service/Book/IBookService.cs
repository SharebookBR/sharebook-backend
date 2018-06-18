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

        object GetTop15NewBooks();
    }
}
