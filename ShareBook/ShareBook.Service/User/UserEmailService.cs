using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using ShareBook.Domain;
using ShareBook.Domain.DTOs;
using ShareBook.Service.AwsSqs;
using ShareBook.Service.Server;

namespace ShareBook.Service
{
    public class UserEmailService : IUserEmailService
    {
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
                LinkForgotMyPassword = $"{_serverSettings.FrontendUrl}/ForgotPassword/{user.HashCodePassword}",
                User = user,
            };
            var html = await _emailTemplate.GenerateHtmlFromTemplateAsync("ForgotPasswordTemplate", vm);

            var title = "Esqueceu sua senha - Sharebook";
            await _emailService.SendAsync(user.Email, user.Name, html, title);
        }

        public async Task SendEmailRequestParentAprovalAsync(RegisterUserDTO userDto, User user)
        {
            var vm = new
            {
                UserName = user.Name,
                AprovalLink = $"{_serverSettings.FrontendUrl}/consentimento-dos-pais/{user.ParentHashCodeAproval}",
            };
            var html = await _emailTemplate.GenerateHtmlFromTemplateAsync("RequestParentAproval", vm);

            var title = "Consentimento dos pais";
            await _emailService.SendAsync(userDto.ParentEmail, "Pais", html, title);
        }

        public async Task SendEmailParentAprovedNotifyUserAsync(User user)
        {
            var vm = new
            {
                SharebookLink = _serverSettings.FrontendUrl
            };
            var html = await _emailTemplate.GenerateHtmlFromTemplateAsync("ParentAprovedNotifyUser", vm);

            var title = "Consentimento dos pais";
            await _emailService.SendAsync(user.Email, user.Name, html, title);
        }

        public async Task SendEmailAnonymizeNotifyAdmsAsync(UserAnonymizeDTO dto)
        {
            var html = await _emailTemplate.GenerateHtmlFromTemplateAsync("AnonymizeNotifyAdms", dto);
            var title = "Anonimização de conta";
            await _emailService.SendToAdminsAsync(html, title);
        }
    }
}
