using ShareBook.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShareBook.Test.Unit.Mocks
{
    public class BookUserMock
    {
        public static BookUser GetDonation(Book book, User requestingUser)
        {
            return new  BookUser()
            {
                Book = book,
                User = requestingUser,
                Reason = "MOTIVO"
            };
        }
    }
}
