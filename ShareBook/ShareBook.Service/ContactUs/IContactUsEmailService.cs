using ShareBook.Domain;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public interface IContactUsEmailService
    {
        Task SendEmailContactUs(ContactUs contactUs);
    }
}
