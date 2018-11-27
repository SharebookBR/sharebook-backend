using ShareBook.Domain;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public interface IBookUsersEmailService
    {
        Task SendEmailBookRequested(BookUser bookUser);

        Task SendEmailBookDonated(BookUser bookUser);

        Task SendEmailBookDonor(BookUser bookUser);
    }
}
