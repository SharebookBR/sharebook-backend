using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ShareBook.Service.AwsSqs.Dto;

namespace ShareBook.Service.AwsSqs;

public class MailSenderHighPriorityQueue : GenericQueue<MailSenderbody>
{
    public MailSenderHighPriorityQueue(IOptions<AwsSqsSettings> awsSqsSettings) : base(awsSqsSettings)
    {
        _queueUrl = $"{_awsSqsSettings.QueueBaseUrl}/{_awsSqsSettings.SendEmailHighPriorityQueue}";
    }
}
