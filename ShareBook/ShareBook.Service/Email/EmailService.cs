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

        public async void Send(string emailRecipient, string nameRecipient, string messageText, string subject, bool copyAdmins)
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
            catch (System.Exception)
            {

               //v2 implementar log para exceptions
            }
            

        }

        private MimeMessage FormatEmail(string emailRecipient, string nameRecipient, string messageText, string subject, bool copyAdmins)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Sharebook", _settings.Username));
            message.To.Add(new MailboxAddress(nameRecipient, emailRecipient));

            if(copyAdmins)
                message.To.Add(new MailboxAddress("Sharebook", _settings.Username));

            message.Subject = subject;
            message.Body = new TextPart("HTML")
            {
                Text = messageText
            };
            return message;
        }
    }
}
