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

        IList<dynamic> FreightOptions();

        IList<Book> Top15NewBooks();
        IList<Book> Random15Books();

        IList<Book> ByTitle(string title);

        IList<Book> ByAuthor(string author);

        IList<Book> GetAll(int page, int items);

        Book BySlug(string slug);

        bool UserRequestedBook(Guid bookId);
    }
}
