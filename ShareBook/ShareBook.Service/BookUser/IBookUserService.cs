using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Service.Generic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShareBook.Service
{
    public interface IBookUserService 
    {
        void Insert(Guid idBook);

        IQueryable<BookUser> Get();
    }
}
