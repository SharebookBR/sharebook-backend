using System.Threading.Tasks;
using ShareBook.Domain;
using ShareBook.Domain.DTOs;

namespace ShareBook.Service
{
    public interface IUserEmailService
    {
         Task SendEmailForgotMyPasswordToUserAsync(User user);
         Task SendEmailUserCancellation(UserCancellationDto dto);
    }
}
