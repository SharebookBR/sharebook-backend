using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ShareBook.Service.AwsSqs.Dto;

namespace ShareBook.Service.AwsSqs;

public class MailSenderHighPriorityQueue : GenericQueue<MailSenderbody>, IAwsSqsQueue<MailSenderbody>
{
    private readonly IConfiguration _configuration;

    public MailSenderHighPriorityQueue(IOptions<AwsSqsSettings> awsSqsSettings, IConfiguration configuration) : base(awsSqsSettings)
    {
        _queueUrl = $"{_awsSqsSettings.QueueBaseUrl}/{_awsSqsSettings.SendEmailHighPriorityQueue}";
        _configuration = configuration;
    }

    public void SendToAdmins(string emailBodyHTML, string emailSubject)
    {
        // TODO: caso o SQS esteja desabilitado, usar o email service diretamente.

        var queueMessage = new MailSenderbody
        {
            CopyAdmins = true,
            BodyHTML = emailBodyHTML,
            Subject = emailSubject,
            Destinations = new List<Destination>
                {
                    new Destination
                    {
                        Email = _configuration["EmailSettings:Username"],
                        Name = "Administradores Sharebook"
                    }
                }
        };

        SendMessage(queueMessage).Wait();
    }
}
