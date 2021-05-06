using ShareBook.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public interface IBookUsersEmailService
    {
       public Task SendEmailBookRequested(List<BookUser> bookUserGroup);

        public Task SendEmailBookDonated(BookUser bookUser);

        public Task SendEmailBookDonatedNotifyDonor(Book book, User winner);

        public Task SendEmailBookDonor(BookUser bookUser, Book bookRequested);

        public Task SendEmailBookInterested(BookUser bookUser, Book book);

        public Task SendEmailDonationDeclined(Book book, BookUser bookUserWinner, List<BookUser> bookUsersDeclined);

        public Task SendEmailDonationCanceled(Book book, List<BookUser> bookUsers);

        public Task SendEmailBookCanceledToAdmins(Book book);

        public Task SendEmailTrackingNumberInformed(BookUser bookUserWinner, Book book);
        public Task SendEmailMaxRequests(Book bookRequested);
    }
}
