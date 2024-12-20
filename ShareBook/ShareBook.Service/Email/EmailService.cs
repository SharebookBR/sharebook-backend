using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
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
        var firstAdm = await _userRepository.Get().Where(u => u.Profile == Domain.Enums.Profile.Administrator).FirstOrDefaultAsync();
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
        var message = await FormatEmailAsync(emailRecipient, nameRecipient, messageText, subject, copyAdmins);

        var client = new SmtpClient();
        
        if (_settings.UseSSL)
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

        client.CheckCertificateRevocation = false;
        await client.ConnectAsync(_settings.HostName, _settings.Port, _settings.UseSSL);
        await client.AuthenticateAsync(_settings.Username, _settings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true); 
    }

    private async Task<MimeMessage> FormatEmailAsync(string emailRecipient, string nameRecipient, string messageText, string subject, bool copyAdmins)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Sharebook", "contato@sharebook.com.br"));
        message.To.Add(new MailboxAddress(nameRecipient, emailRecipient));

        if (copyAdmins)
        {
            var adminsEmails = await FormatEmailGetAdminEmailsAsync();
            message.Cc.AddRange(adminsEmails);
        }

        message.Subject = subject;
        message.Body = new TextPart("HTML")
        {
            Text = messageText
        };
        return message;
    }

    private async Task<InternetAddressList> FormatEmailGetAdminEmailsAsync()
    {
        var admins = await _userRepository.Get()
            .Select(u => new User {
                Email = u.Email,
                Profile = u.Profile
            }
            )
            .Where(u => u.Profile == Domain.Enums.Profile.Administrator)
            .ToListAsync();

        InternetAddressList list = new InternetAddressList();
        foreach (var admin in admins)
        {
            list.Add(MailboxAddress.Parse(admin.Email));
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
        var log = new List<string>();

        if(string.IsNullOrEmpty(_settings.BounceFolder))
        {
            log.Add("Não foi possível processar os emails bounce porque o 'BounceFolder' não está configurado.");
            return log;
        }

        await _imapClient.ConnectAsync(_settings.HostName, _settings.ImapPort, _settings.UseSSL);
        await _imapClient.AuthenticateAsync(_settings.Username, _settings.Password);

        var bounceFolder = await GetBounceFolderAsync();
        await bounceFolder.OpenAsync(FolderAccess.ReadWrite);

        var MAX_EMAILS_TO_PROCESS = 50;
        var items = await bounceFolder.FetchAsync(0, MAX_EMAILS_TO_PROCESS, MessageSummaryItems.UniqueId | MessageSummaryItems.Size | MessageSummaryItems.Flags);

        foreach (var item in items)
        {
            var message = await bounceFolder.GetMessageAsync(item.UniqueId);
            var bounce = new MailBounce(message.Subject, message.TextBody);
            await bounceFolder.AddFlagsAsync(item.UniqueId, MessageFlags.Deleted, true);

            if (bounce.IsBounce)
            {
                log.Add($"Email bounce processado:  subject: {message.Subject}, errorCode: {bounce.ErrorCode}");
                await _ctx.MailBounces.AddAsync(bounce);
            }
            else
            {
                log.Add($"Não vou processar porque NÃO É um email bounce:  subject: {message.Subject}");
            }
   
        }

        await _ctx.SaveChangesAsync();

        // Remove os emails bounce no server
        await bounceFolder.ExpungeAsync();

        await _imapClient.DisconnectAsync(true);

        return log;
    }

    private async Task<IMailFolder?> GetBounceFolderAsync()
    {
        var personal = await _imapClient.GetFolderAsync(_imapClient.PersonalNamespaces[0].Path);
        foreach (var folder in await personal.GetSubfoldersAsync(false))
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

        if (hardBounces.Exists(b => b.Email == email))
            return true;

        if (softBounces.Exists(b => b.Email == email))
            return true;

        return false;
    }
}
