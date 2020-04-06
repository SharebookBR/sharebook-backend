using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using ShareBook.Domain;
using ShareBook.Repository;
using System.Collections.Generic;
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
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    client.Connect(_settings.HostName, _settings.Port, true);
                    client.Authenticate(_settings.Username, _settings.Password);
                    await client.SendAsync(message);
                    client.Disconnect(true);
                }
            }
            catch (System.Exception e)
            {
                //TODO: v2 implementar log para exceptions
                throw e;

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

    }
}
