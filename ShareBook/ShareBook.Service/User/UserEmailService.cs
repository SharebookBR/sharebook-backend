using Microsoft.Extensions.Options;
using ShareBook.Domain;
using ShareBook.Service.Server;

namespace ShareBook.Service
{
    public class UserEmailService : IUserEmailService
    {
        private const string ForgotPasswordTemplate = "ForgotPasswordTemplate";
        private const string ForgotPasswordTitle = "Esqueceu sua senha - Sharebook";

        private readonly IEmailService _emailService;
        private readonly IEmailTemplate _emailTemplate;
        private readonly ServerSettings _serverSettings;

        public UserEmailService(IEmailService emailService, IEmailTemplate emailTemplate, IOptions<ServerSettings> serverSettings)
        {
            _emailService = emailService;
            _emailTemplate = emailTemplate;
            _serverSettings = serverSettings.Value;
        }

        public async void SendEmailForgotMyPasswordToUserAsync(User user)
        {
            var vm = new
            {
                LinkForgotMyPassword = $"{_serverSettings.DefaultUrl}/ForgotPassword/{user.HashCodePassword}",
                User = user,
            };
            var html = await _emailTemplate.GenerateHtmlFromTemplateAsync(ForgotPasswordTemplate, vm);

            await _emailService.Send(user.Email, user.Name, html, ForgotPasswordTitle);
        }
    }
}
