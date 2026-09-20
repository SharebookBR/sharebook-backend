using Microsoft.Extensions.Options;
using ShareBook.Domain;
using ShareBook.Domain.DTOs;
using ShareBook.Service.AwsSqs;
using ShareBook.Service.Server;
using System.Threading.Tasks;

namespace ShareBook.Service;

public class UserEmailService(IEmailService emailService, IEmailTemplate emailTemplate, IOptions<ServerSettings> serverSettings) : IUserEmailService
{
    private readonly IEmailService _emailService = emailService;
    private readonly IEmailTemplate _emailTemplate = emailTemplate;
    private readonly ServerSettings _serverSettings = serverSettings.Value;

    public async Task SendEmailForgotMyPasswordToUserAsync(User user)
    {
        var vm = new
        {
            LinkForgotMyPassword = $"{_serverSettings.FrontendUrl}/ForgotPassword/{user.HashCodePassword}",
            User = user,
        };
        var html = await _emailTemplate.GenerateHtmlFromTemplateAsync("ForgotPasswordTemplate", vm);

        var title = "Crie uma nova senha";
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

        var title = "Autorize o acesso ao Sharebook";
        await _emailService.SendAsync(userDto.ParentEmail, "Pais", html, title);
    }

    public async Task SendEmailParentAprovedNotifyUserAsync(User user)
    {
        var vm = new
        {
            SharebookLink = _serverSettings.FrontendUrl
        };
        var html = await _emailTemplate.GenerateHtmlFromTemplateAsync("ParentAprovedNotifyUser", vm);

        var title = "Seu acesso ao Sharebook foi liberado";
        await _emailService.SendAsync(user.Email, user.Name, html, title);
    }

    public async Task SendEmailAnonymizeNotifyAdmsAsync(UserAnonymizeDTO dto)
    {
        var html = await _emailTemplate.GenerateHtmlFromTemplateAsync("AnonymizeNotifyAdms", dto);
        var title = "Anonimização de conta";
        await _emailService.SendToAdminsAsync(html, title);
    }
}
