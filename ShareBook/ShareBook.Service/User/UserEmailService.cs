using Microsoft.Extensions.Options;

using ShareBook.Domain;
using ShareBook.Domain.DTOs;
using ShareBook.Service.Server;

using System.Threading.Tasks;

namespace ShareBook.Service {
    public class UserEmailService : IUserEmailService
    {
        private const string ForgotPasswordTemplate = "ForgotPasswordTemplate";
        private const string UserCanceledTemplate = "UserCanceledTemplate";
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

        public async Task SendEmailForgotMyPasswordToUserAsync(User user)
        {
            var vm = new
            {
                LinkForgotMyPassword = $"{_serverSettings.DefaultUrl}/ForgotPassword/{user.HashCodePassword}",
                User = user,
            };
            var html = await _emailTemplate.GenerateHtmlFromTemplateAsync(ForgotPasswordTemplate, vm);

            await _emailService.Send(user.Email, user.Name, html, ForgotPasswordTitle);
        }

        public async Task SendEmailUserCancellation(UserCancellationDto dto) {
            var html = await _emailTemplate.GenerateHtmlFromTemplateAsync(UserCanceledTemplate, dto);
            await _emailService.SendToAdmins(html, "Usuário solicitou cancelamento e foi anonimizado!");
        }
    }
}
