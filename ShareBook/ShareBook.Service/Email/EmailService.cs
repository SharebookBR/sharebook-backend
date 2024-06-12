using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;
using Rollbar.DTOs;
using ShareBook.Domain;
using ShareBook.Repository;
using ShareBook.Service.AwsSqs;
using ShareBook.Service.AwsSqs.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShareBook.Service;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly MailSenderLowPriorityQueue _mailSenderLowPriorityQueue;
    private readonly MailSenderHighPriorityQueue _mailSenderHighPriorityQueue;
    private readonly ImapClient _imapClient;

    private readonly ApplicationDbContext _ctx;

    public EmailService(IOptions<EmailSettings> emailSettings, IUserRepository userRepository,
    IConfiguration configuration, MailSenderLowPriorityQueue mailSenderLowPriorityQueue,
    MailSenderHighPriorityQueue mailSenderHighPriorityQueue, ApplicationDbContext ctx)
    {
        _settings = emailSettings.Value;
        _userRepository = userRepository;
        _configuration = configuration;
        _mailSenderLowPriorityQueue = mailSenderLowPriorityQueue;
        _mailSenderHighPriorityQueue = mailSenderHighPriorityQueue;

        _imapClient = new ImapClient();
        _imapClient.CheckCertificateRevocation = false;
        _imapClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
        _ctx = ctx;
    }

    public async Task SendToAdminsAsync(string messageText, string subject)
    {
        var firstAdm = _userRepository.Get().Where(u => u.Profile == Domain.Enums.Profile.Administrator).FirstOrDefault();
        await SendAsync(firstAdm.Email, firstAdm.Name, messageText, subject, copyAdmins: true, highPriority: true);
    }

    public async Task SendAsync(string emailRecipient, string nameRecipient, string messageText, string subject)
        => await SendAsync(emailRecipient, nameRecipient, messageText, subject, copyAdmins: false, highPriority: true);

    public async Task SendAsync(string emailRecipient, string nameRecipient, string messageText, string subject, bool copyAdmins = false, bool highPriority = true)
    {
        var sqsEnabled = bool.Parse(_configuration["AwsSqsSettings:IsActive"]);

        if (!sqsEnabled)
        {
            await SendSmtpAsync(emailRecipient, nameRecipient, messageText, subject, copyAdmins);
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
            await _mailSenderHighPriorityQueue.SendMessageAsync(queueMessage);
        else
            await _mailSenderLowPriorityQueue.SendMessageAsync(queueMessage);

    }

    public async Task SendSmtpAsync(string emailRecipient, string nameRecipient, string messageText, string subject, bool copyAdmins)
    {
        var message = FormatEmail(emailRecipient, nameRecipient, messageText, subject, copyAdmins);

        var client = new SmtpClient();
        
        if (_settings.UseSSL)
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

        client.CheckCertificateRevocation = false;
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
        // TODO: Migrate to async
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

    public async Task TestAsync(string email, string name)
    {
        var subject = "Sharebook - teste de email";
        var message = $"<p>Olá {name},</p> <p>Esse é um email de teste para verificar se o sharebook consegue fazer contato com você. Por favor avise o facilitador quando esse email chegar. Obrigado.</p>";
        await this.SendSmtpAsync(email, name, message, subject, copyAdmins: false);
    }

    public async Task<IList<string>> ProcessBounceMessagesAsync()
    {
        // TODO: Improve async/await
        var log = new List<string>();

        if(string.IsNullOrEmpty(_settings.BounceFolder))
        {
            log.Add("Não foi possível processar os emails bounce porque o 'BounceFolder' não está configurado.");
            return log;
        }

        _imapClient.Connect(_settings.HostName, _settings.ImapPort, true);
        _imapClient.Authenticate(_settings.Username, _settings.Password);

        var bounceFolder = GetBounceFolder();
        await bounceFolder.OpenAsync(FolderAccess.ReadWrite);

        var MAX_EMAILS_TO_PROCESS = 50;
        var items = bounceFolder.Fetch(0, MAX_EMAILS_TO_PROCESS, MessageSummaryItems.UniqueId | MessageSummaryItems.Size | MessageSummaryItems.Flags);

        foreach (var item in items)
        {
            var message = bounceFolder.GetMessage(item.UniqueId);
            var bounce = new MailBounce(message.Subject, message.TextBody);
            bounceFolder.AddFlags(item.UniqueId, MessageFlags.Deleted, true);

            if (bounce.IsBounce)
            {
                log.Add($"Email bounce processado:  subject: {message.Subject}, errorCode: {bounce.ErrorCode}");
                _ctx.MailBounces.Add(bounce);
            }
            else
            {
                log.Add($"Não vou processar porque NÃO É um email bounce:  subject: {message.Subject}");
            }
   
        }

        _ctx.SaveChanges();

        // Remove os emails bounce no server
        bounceFolder.Expunge();

        _imapClient.Disconnect(true);

        return log;
    }

    private IMailFolder? GetBounceFolder()
    {
        var personal = _imapClient.GetFolder(_imapClient.PersonalNamespaces[0]);
        foreach (var folder in personal.GetSubfolders(false))
            if (folder.Name == _settings.BounceFolder)
                return folder;

        return null;
    }

    public async Task<IList<MailBounce>> GetBouncesAsync(IList<string> emails)
    {
        return await _ctx.MailBounces.Where(m => emails.Contains(m.Email)).ToListAsync();
    }

    public bool IsBounce(string email, IList<MailBounce> bounces)
    {
        var hardBounces = bounces.Where(b => !b.IsSoft).ToList();
        var softBounces = bounces.Where(b => b.IsSoft && b.CreationDate > DateTime.Now.AddDays(-1)).ToList();

        if (hardBounces.Any(b => b.Email == email))
            return true;

        if (softBounces.Any(b => b.Email == email))
            return true;

        return false;
    }
}
