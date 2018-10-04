using System.Threading.Tasks;
using ShareBook.Domain.Entities;

namespace ShareBook.Service
{
    public interface IBooksEmailService
    {
        Task SendEmailNewBookInserted(Book book);

        Task SendEmailBookApproved(Book book);
    }
}