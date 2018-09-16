using ShareBook.Domain;
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
        private readonly IUserService _userService;
        private readonly IEmailTemplate _emailTemplate;

        public ContactUsEmailService(IEmailService emailService, IUserService userService, IEmailTemplate emailTemplate)
        {
            _emailService = emailService;
            _userService = userService;
            _emailTemplate = emailTemplate;

        }
        public async Task SendEmailContactUs(ContactUs contactUs)
        {
            
            var administrators = _userService.GetAllAdministrators();

            foreach (var admin in administrators)
                await SendEmailContactUsToAdministrator(contactUs, admin);

            await SendEmailNotificationToUser(contactUs);
        }
        private async Task SendEmailContactUsToAdministrator(ContactUs contactUs, User administrator)
        {
            var vm = new
            {
                ContactUs = contactUs,
                Administrator = administrator
            };

            var html = await _emailTemplate.GenerateHtmlFromTemplateAsync(ContactUsTemplate, vm);
            bool copyAdmins = false;
            _emailService.Send(administrator.Email, administrator.Name, html, ContactUsTitle, copyAdmins);
        }
        private async Task SendEmailNotificationToUser(ContactUs contactUs)
        {
            var vm = new
            {
                ContactUs = contactUs
            };
            bool copyAdmins = true;
            var html = await _emailTemplate.GenerateHtmlFromTemplateAsync(ContactUsNotificationTemplate, vm);
            _emailService.Send(contactUs.Email, contactUs.Name, html, ContactUsNotificationTitle, copyAdmins);
        }

    }
}
