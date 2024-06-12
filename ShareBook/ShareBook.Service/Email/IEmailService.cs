using ShareBook.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public interface IEmailService
    {
        Task SendToAdminsAsync(string messageText, string subject);
        Task SendAsync(string emailRecipient, string nameRecipient, string messageText, string subject);
        Task SendAsync(string emailRecipient, string nameRecipient, string messageText, string subject, bool copyAdmins, bool highPriority);
        Task SendSmtpAsync(string emailRecipient, string nameRecipient, string messageText, string subject, bool copyAdmins);
        Task TestAsync(string email, string name);

        Task<IList<string>> ProcessBounceMessagesAsync();
        Task<IList<MailBounce>> GetBouncesAsync(IList<string> emails);
        bool IsBounce(string email, IList<MailBounce> bounces);
    }
}
