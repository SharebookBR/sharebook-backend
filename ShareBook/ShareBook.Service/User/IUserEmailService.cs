using System.Threading.Tasks;
using ShareBook.Domain;
using ShareBook.Domain.DTOs;

namespace ShareBook.Service
{
    public interface IUserEmailService
    {
        Task SendEmailForgotMyPasswordToUserAsync(User user);
        void SendEmailRequestParentAproval(RegisterUserDTO userDto, User user);
        Task SendEmailParentAprovedNotifyUserAsync(User user);
        void SendEmailAnonymizeNotifyAdms(UserAnonymizeDTO dto);
    }
}
