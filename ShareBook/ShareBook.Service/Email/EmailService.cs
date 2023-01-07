using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;
using ShareBook.Domain;
using ShareBook.Repository;
using ShareBook.Service.AwsSqs;
using ShareBook.Service.AwsSqs.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly MailSenderLowPriorityQueue _mailSenderLowPriorityQueue;
        private readonly MailSenderHighPriorityQueue _mailSenderHighPriorityQueue;

        public EmailService(IOptions<EmailSettings> emailSettings, IUserRepository userRepository, 
        IConfiguration configuration, MailSenderLowPriorityQueue mailSenderLowPriorityQueue, 
        MailSenderHighPriorityQueue mailSenderHighPriorityQueue)
        {
            _settings = emailSettings.Value;
            _userRepository = userRepository;
            _configuration = configuration;
            _mailSenderLowPriorityQueue = mailSenderLowPriorityQueue;
            _mailSenderHighPriorityQueue = mailSenderHighPriorityQueue;
        }

        public async Task SendToAdmins(string messageText, string subject)
        {
            var firstAdm = _userRepository.Get().Where(u => u.Profile == Domain.Enums.Profile.Administrator).FirstOrDefault();
            await Send(firstAdm.Email, firstAdm.Name, messageText, subject, copyAdmins: true, highPriority: true);
        }

        public async Task Send(string emailRecipient, string nameRecipient, string messageText, string subject)
            => await Send(emailRecipient, nameRecipient, messageText, subject, copyAdmins: false, highPriority: true);

        public async Task Send(string emailRecipient, string nameRecipient, string messageText, string subject, bool copyAdmins = false, bool highPriority = true)
        {
            var sqsEnabled = bool.Parse(_configuration["AwsSqsSettings:IsActive"]);

            if (!sqsEnabled)
            {
                await SendSmtp(emailRecipient, nameRecipient, messageText, subject, copyAdmins);
                return;
            }

            var queueMessage = new MailSenderbody{
                CopyAdmins = copyAdmins,
                Subject = subject,
                BodyHTML = messageText,
                Destinations = new List<Destination>{
                    {
                        new Destination {
                            Name = nameRecipient,
                            Email = emailRecipient
                        }
                    }
                }
            };

            if (highPriority)
                await _mailSenderHighPriorityQueue.SendMessage(queueMessage);
            else
                await _mailSenderLowPriorityQueue.SendMessage(queueMessage);

        }

        public async Task SendSmtp(string emailRecipient, string nameRecipient, string messageText, string subject, bool copyAdmins)
        {
            var message = FormatEmail(emailRecipient, nameRecipient, messageText, subject, copyAdmins);

            var client = new SmtpClient();
            
            if (_settings.UseSSL)
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            client.Connect(_settings.HostName, _settings.Port, _settings.UseSSL);
            client.Authenticate(_settings.Username, _settings.Password);
            await client.SendAsync(message);
            client.Disconnect(true); 
        }

        private MimeMessage FormatEmail(string emailRecipient, string nameRecipient, string messageText, string subject, bool copyAdmins)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Sharebook", "contato@sharebook.com.br"));
            message.To.Add(new MailboxAddress(nameRecipient, emailRecipient));

            if (copyAdmins)
            {
                var adminsEmails = FormatEmailGetAdminEmails();
                message.Cc.AddRange(adminsEmails);
            }

            message.Subject = subject;
            message.Body = new TextPart("HTML")
            {
                Text = messageText
            };
            return message;
        }

        private InternetAddressList FormatEmailGetAdminEmails()
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
            await this.SendSmtp(email, name, message, subject, copyAdmins: false);
        }

    }
}
