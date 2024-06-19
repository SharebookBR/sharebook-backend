using ShareBook.Domain;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public class ContactUsEmailService : IContactUsEmailService
    {
        private const string ContactUsTemplate = "ContactUsTemplate";
        public const string ContactUsTitle = "Fale Conosco - Sharebook";
        private const string ContactUsNotificationTemplate = "ContactUsNotificationTemplate";
        public const string ContactUsNotificationTitle = "Fale Conosco - Sharebook";

        private readonly IEmailService _emailService;

        private readonly IEmailTemplate _emailTemplate;


        public ContactUsEmailService(IEmailService emailService, IEmailTemplate emailTemplate)
        {
            _emailService = emailService;
            _emailTemplate = emailTemplate;
        }
        public async Task SendEmailContactUsAsync(ContactUs contactUs)
        {
            await SendEmailContactUsToAdministratorAsync(contactUs);

            await SendEmailNotificationToUserAsync(contactUs);
        }
        private async Task SendEmailContactUsToAdministratorAsync(ContactUs contactUs)
        {
            var html = await _emailTemplate.GenerateHtmlFromTemplateAsync(ContactUsTemplate, contactUs);
            await _emailService.SendToAdminsAsync(html, ContactUsTitle);
        }
        private async Task SendEmailNotificationToUserAsync(ContactUs contactUs)
        {
            var html = await _emailTemplate.GenerateHtmlFromTemplateAsync(ContactUsNotificationTemplate, contactUs);
            await _emailService.SendAsync(contactUs.Email, contactUs.Name, html, ContactUsNotificationTitle, copyAdmins: false, highPriority: true);
        }
    }
}
