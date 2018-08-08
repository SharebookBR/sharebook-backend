using ShareBook.Domain;
using System;
using System.Collections.Generic;

namespace ShareBook.Service
{
    public interface IBookUserService 
    {
        void Insert(Guid bookId);

        IList<User> GetGranteeUsersByBookId(Guid bookId);

        void DonateBook(Guid bookId, Guid userId);

        void DeniedBookUsers(Guid bookId);
    }
}
