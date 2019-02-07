using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Service.Generic;
using System;
using System.Collections.Generic;

namespace ShareBook.Service
{
    public interface IBookService : IBaseService<Book>
    {
        Result<Book> Approve(Guid bookId, DateTime? chooseDate);

        void HideBook(Guid bookId);

        IList<dynamic> FreightOptions();

        IList<Book> Top15NewBooks();

        IList<Book> Random15Books();

        PagedList<Book> ByTitle(string title, int page, int itemsPerPage);

        PagedList<Book> ByAuthor(string author, int page, int itemsPerPage);

        PagedList<Book> FullSearch(string criteria, int page, int itemsPerPage, bool isAdmin = false);

        PagedList<Book> ByCategoryId(Guid categoryId, int page, int items);

        IList<Book> GetAll(int page, int items);

        Book BySlug(string slug);

        bool UserRequestedBook(Guid bookId);

        IList<Book> GetUserDonations(Guid userId);

        IList<Book> GetBooksChooseDateIsToday();

        IList<Book> GetBooksChooseDateIsLate();

        void AddFacilitatorNotes(Guid bookId, string facilitatorNotes);

        Book GetBookWithAllUsers(Guid bookId);
    }
}
