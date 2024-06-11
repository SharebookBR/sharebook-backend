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
                LinkForgotMyPassword = $"{_serverSettings.DefaultUrl}/ForgotPassword/{user.HashCodePassword}",
                User = user,
            };
            var html = await _emailTemplate.GenerateHtmlFromTemplateAsync("ForgotPasswordTemplate", vm);

            var title = "Esqueceu sua senha - Sharebook";
            await _emailService.Send(user.Email, user.Name, html, title);
        }

        public void SendEmailRequestParentAproval(RegisterUserDTO userDto, User user)
        {
            var vm = new
            {
                UserName = user.Name,
                AprovalLink = $"{_serverSettings.DefaultUrl}/consentimento-dos-pais/{user.ParentHashCodeAproval}",
            };
            // TODO: Remove "GetAwaiter().GetResult()"
            var html = _emailTemplate.GenerateHtmlFromTemplateAsync("RequestParentAproval", vm).GetAwaiter().GetResult();

            var title = "Consentimento dos pais";
            // TODO: Remove ".Wait()"
            _emailService.Send(userDto.ParentEmail, "Pais", html, title).Wait();
        }

        public void SendEmailParentAprovedNotifyUser(User user)
        {
            var vm = new
            {
                SharebookLink = _serverSettings.DefaultUrl
            };
            // TODO: Remove "GetAwaiter().GetResult()"
            var html = _emailTemplate.GenerateHtmlFromTemplateAsync("ParentAprovedNotifyUser", vm).GetAwaiter().GetResult();

            var title = "Consentimento dos pais";
            _emailService.Send(user.Email, user.Name, html, title).Wait();
        }

        public void SendEmailAnonymizeNotifyAdms(UserAnonymizeDTO dto)
        {
            // TODO: Remove "GetAwaiter().GetResult()"
            var html = _emailTemplate.GenerateHtmlFromTemplateAsync("AnonymizeNotifyAdms", dto).GetAwaiter().GetResult();
            var title = "Anonimização de conta";
            _emailService.SendToAdmins(html, title);
        }
    }
}
