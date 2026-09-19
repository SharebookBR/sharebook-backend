using Microsoft.Extensions.Options;
using ShareBook.Service.AwsSqs.Dto;

namespace ShareBook.Service.AwsSqs;

public class NewBookQueue : GenericQueue<NewBookBody>
{

    public NewBookQueue(IOptions<AwsSqsSettings> awsSqsSettings) : base(awsSqsSettings)
    {
        _queueUrl = $"{_awsSqsSettings.QueueBaseUrl}/{_awsSqsSettings.NewBookQueue}";
    }

    // pra poder mockar
    public NewBookQueue() : base(null) {}
}


        

