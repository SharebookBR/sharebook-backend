using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ShareBook.Service
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _settings = emailSettings.Value;
        }

        public void Send(string emailRecipient, string nameRecipient, string messageText, string subject)
        {
            var message = FormatEmail(emailRecipient, nameRecipient, messageText, subject);

            using (var client = new SmtpClient())
            { 
            
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                client.Connect(_settings.HostName, _settings.Port, false);

              
                client.Authenticate(_settings.Username, _settings.Password);

                client.Send(message);
                client.Disconnect(true);
            }
        }

        private MimeMessage FormatEmail(string emailRecipient, string nameRecipient, string messageText, string subject)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(nameRecipient, emailRecipient));
            message.Subject = subject;
            message.Body = new TextPart("HTML")
            {
                Text = messageText
            };
            return message;
        }
    }
}
