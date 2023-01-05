using ShareBook.Domain;
using ShareBook.Domain.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public interface IBookUsersEmailService
    {
        Task SendEmailBookDonated(BookUser bookUser);

        Task SendEmailBookDonatedNotifyDonor(Book book, User winner);

        Task SendEmailBookDonor(BookUser bookUser, Book bookRequested);

        Task SendEmailBookInterested(BookUser bookUser, Book book);

        Task SendEmailDonationDeclined(Book book, BookUser bookUserWinner, List<BookUser> bookUsersDeclined);

        Task SendEmailDonationCanceled(Book book, List<BookUser> bookUsers);

        Task SendEmailBookCanceledToAdminsAndDonor(BookCancelationDTO dto);

        Task SendEmailTrackingNumberInformed(BookUser bookUserWinner, Book book);

        Task SendEmailMaxRequests(Book bookRequested);
    }
}
