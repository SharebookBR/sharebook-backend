using ShareBook.Domain.Entities;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public interface IBookUsersEmailService
    {
        Task SendEmailBookRequested(BookUser bookUser);

        Task SendEmailBookDonated(BookUser bookUser);
    }
}
