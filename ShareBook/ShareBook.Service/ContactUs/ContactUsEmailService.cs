using ShareBook.Domain;
using ShareBook.Service.AwsSqs;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public class ContactUsEmailService : IContactUsEmailService
    {
        private const string ContactUsTemplate = "ContactUsTemplate";
        private const string ContactUsTitle = "Fale Conosco - Sharebook";
        private const string ContactUsNotificationTemplate = "ContactUsNotificationTemplate";
        private const string ContactUsNotificationTitle = "Fale Conosco - Sharebook";

        private readonly IEmailService _emailService;

        private readonly IEmailTemplate _emailTemplate;


        public ContactUsEmailService(IEmailService emailService, IEmailTemplate emailTemplate)
        {
            _emailService = emailService;
            _emailTemplate = emailTemplate;
        }
        public async Task SendEmailContactUs(ContactUs contactUs)
        {
            await SendEmailContactUsToAdministrator(contactUs);

            await SendEmailNotificationToUser(contactUs);
        }
        private async Task SendEmailContactUsToAdministrator(ContactUs contactUs)
        {
            var html = await _emailTemplate.GenerateHtmlFromTemplateAsync(ContactUsTemplate, contactUs);
            await _emailService.SendToAdmins(html, ContactUsTitle);
        }
        private async Task SendEmailNotificationToUser(ContactUs contactUs)
        {
            var html = await _emailTemplate.GenerateHtmlFromTemplateAsync(ContactUsNotificationTemplate, contactUs);
            await _emailService.Send(contactUs.Email, contactUs.Name, html, ContactUsNotificationTitle, copyAdmins: false, highPriority: true);
        }
    }
}
