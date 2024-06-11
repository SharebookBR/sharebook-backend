using System.Threading.Tasks;
using ShareBook.Domain;

namespace ShareBook.Service
{
    public interface IBooksEmailService
    {
        Task SendEmailNewBookInsertedAsync(Book book);

        Task SendEmailBookApprovedAsync(Book book);

        Task SendEmailBookReceivedAsync(Book book);
    }
}