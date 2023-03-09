using ShareBook.Domain;
using ShareBook.Service.AwsSqs.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public interface IEmailService
    {
        Task SendToAdmins(string messageText, string subject);
        Task Send(string emailRecipient, string nameRecipient, string messageText, string subject);
        Task Send(string emailRecipient, string nameRecipient, string messageText, string subject, bool copyAdmins, bool highPriority);
        Task SendSmtp(string emailRecipient, string nameRecipient, string messageText, string subject, bool copyAdmins);
        Task Test(string email, string name);

        Task<IList<string>> ProcessBounceMessages();
        Task<IList<MailBounce>> GetBounces(IList<string> emails);
        bool IsBounce(string email, IList<MailBounce> bounces);
    }
}
