﻿using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Domain.DTOs;
using ShareBook.Domain.Enums;
using ShareBook.Service.Generic;
using System;
using System.Collections.Generic;

namespace ShareBook.Service
{
    public interface IBookService : IBaseService<Book>
    {
        void Approve(Guid bookId, DateTime? chooseDate);

        void Received(Guid bookId, Guid winnerUserId);
        void UpdateBookStatus(Guid bookId, BookStatus bookStatus);

        IList<dynamic> FreightOptions();

        IList<Book> AvailableBooks();

        IList<Book> Random15Books();

        IList<Book> Random15EBooks();

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

        void RenewChooseDate(Guid bookId);
        BookTotalStatusDTO GetTotalStatus();
        bool RevokeBookToWaitingApproval(Guid bookId);
    }
}
