using ShareBook.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShareBook.Test.Unit.Mocks
{
    public class BookMock
    {
        public static Book GetLordTheRings(User user)
        {
           return new Book()
            {
                Title = "Lord of the Rings",
                Author = "J. R. R. Tolkien",
                ImageSlug = "lotr.png",
                ImageBytes = Encoding.UTF8.GetBytes("STRINGBASE64"),
                User = user,
                CategoryId = Guid.NewGuid(),
                Approved = true,
                Canceled = false
           };
        }


        public static Book GetLordTheRings()
        {
            return new Book()
            {
                Title = "Lord of the Rings",
                Author = "J. R. R. Tolkien",
                ImageSlug = "lotr.png",
                ImageBytes = Encoding.UTF8.GetBytes("STRINGBASE64"),
                CategoryId = Guid.NewGuid(),
                UserId = new Guid("5489A967-9320-4350-E6FC-08D5CC8498F3"),
                Approved = true,
                Canceled = false
            };
        }
    }
}
