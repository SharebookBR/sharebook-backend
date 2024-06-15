using ShareBook.Domain;
using ShareBook.Domain.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public interface IBookUsersEmailService
    {
        Task SendEmailBookDonatedAsync(BookUser bookUser);

        Task SendEmailBookDonatedNotifyDonorAsync(Book book, User winner);

        Task SendEmailBookDonorAsync(BookUser bookUser, Book bookRequested);

        Task SendEmailBookInterestedAsync(BookUser bookUser, Book book);

        Task SendEmailDonationDeclinedAsync(Book book, BookUser bookUserWinner, List<BookUser> bookUsersDeclined);

        Task SendEmailDonationCanceledAsync(Book book, List<BookUser> bookUsers);

        Task SendEmailBookCanceledToAdminsAndDonorAsync(BookCancelationDTO dto);

        Task SendEmailTrackingNumberInformedAsync(BookUser bookUserWinner, Book book);

        Task SendEmailMaxRequestsAsync(Book bookRequested);
    }
}
