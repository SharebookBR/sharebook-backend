using ShareBook.Domain;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public interface IContactUsEmailService
    {
        Task SendEmailContactUsAsync(ContactUs contactUs);
    }
}
