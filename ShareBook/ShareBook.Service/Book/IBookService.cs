using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Service.Generic;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace ShareBook.Service
{
    public interface IBookService : IBaseService<Book>
    {
        Result<Book> Approve(Guid bookId);

        List<ExpandoObject> GetAllFreightOptions();
    }
}
