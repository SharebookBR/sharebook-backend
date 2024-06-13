using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Domain.DTOs;
using ShareBook.Domain.Enums;
using ShareBook.Service.Generic;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public interface IBookService : IBaseService<Book>
    {
        Task ApproveAsync(Guid bookId, DateTime? chooseDate);

        Task ReceivedAsync(Guid bookId, Guid winnerUserId);
        Task UpdateBookStatusAsync(Guid bookId, BookStatus bookStatus);

        IList<dynamic> FreightOptions();

        Task<IList<Book>> AvailableBooksAsync();

        Task<IList<Book>> Random15BooksAsync();

        Task<IList<Book>> Random15EBooksAsync();

        Task<PagedList<Book>> FullSearchAsync(string criteria, int page, int itemsPerPage, bool isAdmin = false);

        Task<PagedList<Book>> ByCategoryIdAsync(Guid categoryId, int page, int items);

        Task<IList<Book>> GetAllAsync(int page, int items);

        Task<Book> BySlugAsync(string slug);

        Task<bool> UserRequestedBookAsync(Guid bookId);

        Task<IList<Book>> GetUserDonationsAsync(Guid userId);

        Task<IList<Book>> GetBooksChooseDateIsTodayAsync();

        Task<IList<Book>> GetBooksChooseDateIsLateAsync();

        Task<IList<Book>> GetBooksChooseDateIsTodayOrLateAsync();

        Task AddFacilitatorNotesAsync(Guid bookId, string facilitatorNotes);

        Task<Book> GetBookWithAllUsersAsync(Guid bookId);

        Task RenewChooseDateAsync(Guid bookId);
        Task<BookStatsDTO> GetStatsAsync();
    }
}
