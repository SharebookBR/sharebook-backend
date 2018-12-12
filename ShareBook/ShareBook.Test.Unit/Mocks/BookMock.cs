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
                Category = new Category(Guid.NewGuid())
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
                Category = new Category(Guid.NewGuid()),
                User = new User(new Guid("5489A967-9320-4350-E6FC-08D5CC8498F3"))
            };
        }
    }
}
