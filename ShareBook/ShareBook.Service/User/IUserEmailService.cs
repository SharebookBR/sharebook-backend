using ShareBook.Domain;
using ShareBook.Domain.DTOs;
using System.Threading.Tasks;

namespace ShareBook.Service;

public interface IUserEmailService
{
    Task SendEmailForgotMyPasswordToUserAsync(User user);
    Task SendEmailRequestParentAprovalAsync(RegisterUserDTO userDto, User user);
    Task SendEmailParentAprovedNotifyUserAsync(User user);
    Task SendEmailAnonymizeNotifyAdmsAsync(UserAnonymizeDTO dto);
}
