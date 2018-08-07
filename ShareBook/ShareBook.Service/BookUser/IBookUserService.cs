using ShareBook.Domain;
using System;
using System.Collections.Generic;

namespace ShareBook.Service
{
    public interface IBookUserService 
    {
        void Insert(Guid idBook);

        IList<User> GetGranteeUsersByBookId(Guid bookId);
    }
}
