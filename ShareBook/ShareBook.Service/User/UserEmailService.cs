using ShareBook.Domain;

namespace ShareBook.Service
{
    public class UserEmailService : IUserEmailService
    {
        private const string ForgotPasswordTemplate = "ForgotPasswordTemplate";
        private const string ForgotPasswordTitle = "Esqueceu sua senha - Sharebook";

        private readonly IEmailService _emailService;
        private readonly IEmailTemplate _emailTemplate;

        public UserEmailService(IEmailService emailService, IEmailTemplate emailTemplate)
        {
            _emailService = emailService;
            _emailTemplate = emailTemplate;
        }

        public async void SendEmailForgotMyPasswordToUserAsync(User user)
        {
            var html = await _emailTemplate.GenerateHtmlFromTemplateAsync(ForgotPasswordTemplate, user);
            bool copyAdmins = false;
            await _emailService.Send(user.Email, user.Name, html, ForgotPasswordTitle, copyAdmins);
        }
    }
}
