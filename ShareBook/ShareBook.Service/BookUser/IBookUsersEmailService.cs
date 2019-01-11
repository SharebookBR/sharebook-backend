using ShareBook.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public interface IBookUsersEmailService
    {
        Task SendEmailBookRequested(BookUser bookUser);

        Task SendEmailBookDonated(BookUser bookUser);

        Task SendEmailBookDonor(BookUser bookUser);

        Task SendEmailDonationDeclined(Book book, BookUser bookUserWinner, List<BookUser> bookUsersDeclined);

        Task SendEmailDonationCanceled(Book book, List<BookUser> bookUsers);

        Task SendEmailBookCanceledToAdmins(Book book);
    }
}
