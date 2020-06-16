using System.Threading.Tasks;
using ShareBook.Domain;

namespace ShareBook.Service
{
    public interface IBooksEmailService
    {
        Task SendEmailNewBookInserted(Book book);

        void SendEmailBookApproved(Book book);

        Task SendEmailBookToInterestedUsers(Book book);

    }
}