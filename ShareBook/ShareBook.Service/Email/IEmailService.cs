using System.Threading.Tasks;

namespace ShareBook.Service
{
    public interface IEmailService
    {
        Task SendToAdmins(string messageText, string subject);
        Task Send(string emailRecipient, string nameRecipient, string messageText, string subject);
        Task Send(string emailRecipient, string nameRecipient, string messageText, string subject, bool copyAdmins = false);
    }
}
