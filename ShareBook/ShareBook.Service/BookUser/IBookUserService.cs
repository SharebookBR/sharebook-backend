using ShareBook.Domain.Entities;
using System;
using System.Collections.Generic;

namespace ShareBook.Service
{
    public interface IBookUserService 
    {
        void Insert(Guid bookId, string reason);

        IList<User> GetGranteeUsersByBookId(Guid bookId);

        void DonateBook(Guid bookId, Guid userId, string note);

        void DeniedBookUsers(Guid bookId);

        IList<BookUser> GetRequestsByUser();
    }
}
