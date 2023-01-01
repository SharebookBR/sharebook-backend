using Microsoft.Extensions.Options;
using ShareBook.Service.AwsSqs.Dto;

namespace ShareBook.Service.AwsSqs;

public class MailSenderLowPriorityQueue : GenericQueue<MailSenderbody>, IAwsSqsQueue<MailSenderbody>
{
    public MailSenderLowPriorityQueue(IOptions<AwsSqsSettings> awsSqsSettings) : base(awsSqsSettings)
    {
        _queueUrl = $"{_awsSqsSettings.QueueBaseUrl}/{_awsSqsSettings.SendEmailLowPriorityQueue}";
    }
}
