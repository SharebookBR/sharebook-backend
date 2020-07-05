using System.Threading.Tasks;
using ShareBook.Domain;

namespace ShareBook.Service
{
    public interface IUserEmailService
    {
         Task SendEmailForgotMyPasswordToUserAsync(User user);
    }
}
