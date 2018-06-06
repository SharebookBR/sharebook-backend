using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Service.Generic;
using System;

namespace ShareBook.Service
{
    public interface IBookService : IBaseService<Book>
    {
        Result<Book> Approve(Guid bookId);
    }
}
