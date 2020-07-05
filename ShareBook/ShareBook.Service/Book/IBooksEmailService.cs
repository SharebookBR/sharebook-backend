using System.Threading.Tasks;
using ShareBook.Domain;

namespace ShareBook.Service
{
    public interface IBooksEmailService
    {
        Task SendEmailNewBookInserted(Book book);

        Task SendEmailBookApproved(Book book);

        Task SendEmailBookToInterestedUsers(Book book);

        void SendEmailBookReceived(Book book);

    }
}