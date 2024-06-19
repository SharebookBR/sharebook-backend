using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public interface IBookUserService 
    {
        Task InsertAsync(Guid bookId, string reason);

        Task<IList<User>> GetGranteeUsersByBookIdAsync(Guid bookId);

        Task<IList<BookUser>> GetRequestersListAsync(Guid bookId);

        Task DonateBookAsync(Guid bookId, Guid userId, string note);

        Task DeniedBookUsersAsync(Guid bookId);

        Task<PagedList<BookUser>> GetRequestsByUserAsync(int page, int items);

        /// <summary>
        /// Comunicar os interessados não escolhidos sobre a finalização da doação. e quem ganhou o livro
        /// </summary>
        /// <param name="bookId"></param>
        Task NotifyInterestedAboutBooksWinnerAsync(Guid bookId);

        Task<Result<Book>> CancelAsync(BookCancelationDTO dto);

        Task InformTrackingNumberAsync(Guid bookId, string trackingNumber);
        Task<BookUser> GetRequestAsync(Guid requestId);
        Task<bool> CancelRequestAsync(BookUser request);
    }
}
