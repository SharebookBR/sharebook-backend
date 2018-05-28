using MailKit.Net.Smtp;
using MimeKit;

namespace ShareBook.Service.Email
{
    public class EmailService : IEmailService
    {
        public void Send(string emailRecipient, string nameRecipient, string messageText, string subject)
        {
            var message = FormatEmail(emailRecipient, nameRecipient, messageText, subject);

            using (var client = new SmtpClient())
            {
                // TODO - passar os valores por variaveis de ambiente, ou arquivos de configuração...
                string hostSMTP = string.Empty;
                string userNameSMTP = string.Empty;
                string passwordSMTP = string.Empty;

               
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                client.Connect(hostSMTP, 587, false);

              
                client.Authenticate(userNameSMTP, passwordSMTP);

                client.Send(message);
                client.Disconnect(true);
            }
        }

        private MimeMessage FormatEmail(string emailRecipient, string nameRecipient, string messageText, string subject)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(nameRecipient, emailRecipient));
            message.Subject = subject;
            message.Body = new TextPart("plain")
            {
                Text = messageText
            };
            return message;
        }
    }
}
