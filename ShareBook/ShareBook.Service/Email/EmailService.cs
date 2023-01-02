using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using Rollbar;
using ShareBook.Domain;
using ShareBook.Repository;
using System.Linq;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly IUserRepository _userRepository;

        public EmailService(IOptions<EmailSettings> emailSettings, IUserRepository userRepository)
        {
            _settings = emailSettings.Value;
            _userRepository = userRepository;
        }

        public async Task SendToAdmins(string messageText, string subject)
        {
            await Send(_settings.Username, "Administradores Sharebook", messageText, subject, true);
        }

        public async Task Send(string emailRecipient, string nameRecipient, string messageText, string subject)
            => await Send(emailRecipient, nameRecipient, messageText, subject, false);

        public async Task Send(string emailRecipient, string nameRecipient, string messageText, string subject, bool copyAdmins = false)
        {
            var message = FormatEmail(emailRecipient, nameRecipient, messageText, subject, copyAdmins);
            try
            {
                using (var client = new SmtpClient())
                {
                    if (_settings.UseSSL)
                    {
                        client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    }
                    
                    client.Connect(_settings.HostName, _settings.Port, _settings.UseSSL);
                    client.Authenticate(_settings.Username, _settings.Password);
                    await client.SendAsync(message);
                    client.Disconnect(true);
                }
            }
            catch (System.Exception e)
            {
                RollbarLocator.RollbarInstance.Error(e);
            }
        }



        private MimeMessage FormatEmail(string emailRecipient, string nameRecipient, string messageText, string subject, bool copyAdmins)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Sharebook", _settings.Username));
            message.To.Add(new MailboxAddress(nameRecipient, emailRecipient));

            if (copyAdmins)
            {
                var adminsEmails = GetAdminEmails();
                message.Cc.AddRange(adminsEmails);
            }

            message.Subject = subject;
            message.Body = new TextPart("HTML")
            {
                Text = messageText
            };
            return message;
        }

        private InternetAddressList GetAdminEmails()
        {
            var admins = _userRepository.Get()
                .Select(u => new User {
                    Email = u.Email,
                    Profile = u.Profile
                }
                )
                .Where(u => u.Profile == Domain.Enums.Profile.Administrator)
                .ToList();

            InternetAddressList list = new InternetAddressList();
            foreach (var admin in admins)
            {
                list.Add(new MailboxAddress(admin.Email));
            }

            return list;
        }

        public async Task Test(string email, string name)
        {
            var subject = "Sharebook - teste de email";
            var message = $"<p>Olá {name},</p> <p>Esse é um email de teste para verificar se o sharebook consegue fazer contato com você. Por favor avise o facilitador quando esse email chegar. Obrigado.</p>";
            await this.Send(email, name, message, subject);
        }

    }
}
