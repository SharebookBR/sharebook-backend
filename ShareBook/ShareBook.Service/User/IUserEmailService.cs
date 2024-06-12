using System.Threading.Tasks;
using ShareBook.Domain;
using ShareBook.Domain.DTOs;

namespace ShareBook.Service
{
    public interface IUserEmailService
    {
        Task SendEmailForgotMyPasswordToUserAsync(User user);
        Task SendEmailRequestParentAprovalAsync(RegisterUserDTO userDto, User user);
        Task SendEmailParentAprovedNotifyUserAsync(User user);
        Task SendEmailAnonymizeNotifyAdmsAsync(UserAnonymizeDTO dto);
    }
}
