using ShareBook.Domain;

namespace ShareBook.Service
{
    public interface IUserEmailService
    {
         void SendEmailForgotMyPasswordToUserAsync(User user);
    }
}
