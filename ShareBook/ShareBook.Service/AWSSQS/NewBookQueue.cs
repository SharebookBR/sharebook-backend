using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;
using ShareBook.Domain;
using ShareBook.Service.AwsSqs.Dto;
using System;
using System.Threading.Tasks;

namespace ShareBook.Service.AwsSqs;

public class NewBookQueue : IAwsSqsQueue<NewBookMessage>
{
    private readonly AwsSqsSettings _AwsSqsSettings;
    private readonly AmazonSQSClient _amazonSQSClient;

    public NewBookQueue(IOptions<AwsSqsSettings> AWSSQSSettings)
    {
        _AwsSqsSettings = AWSSQSSettings.Value;

        if (_AwsSqsSettings.IsActive)
        {
            // usando padrão Reflection
            var region = (Amazon.RegionEndpoint)typeof(Amazon.RegionEndpoint).GetField(_AwsSqsSettings.Region).GetValue(null);

            var awsCreds = new BasicAWSCredentials(AWSSQSSettings.Value.AccessKey, AWSSQSSettings.Value.SecretKey);
            _amazonSQSClient = new AmazonSQSClient(awsCreds, region);
        }
    }

    // pra poder mockar
    public NewBookQueue(){}

    
    public async Task SendMessage(NewBookMessage message)
    {
        if (!_AwsSqsSettings.IsActive)
            return;

        var request = new SendMessageRequest
        {
            DelaySeconds = (int)TimeSpan.FromSeconds(5).TotalSeconds,
            MessageBody = System.Text.Json.JsonSerializer.Serialize(message),
            QueueUrl = $"{_AwsSqsSettings.QueueBaseUrl}/{_AwsSqsSettings.NewBookQueue}"
        };

        await _amazonSQSClient.SendMessageAsync(request);
    }

    public async Task<NewBookMessage> GetMessage()
    {
        if (!_AwsSqsSettings.IsActive)
        {
            throw new Exception("Serviço aws está desabilitado no appsettings.");
        }

        var url = $"{_AwsSqsSettings.QueueBaseUrl}/{_AwsSqsSettings.NewBookQueue}";
        var receiveMessageRequest = new ReceiveMessageRequest(url);

        var result = await _amazonSQSClient.ReceiveMessageAsync(receiveMessageRequest);

        if (result.Messages.Count > 0)
        {
            var firstMessageTemp = result.Messages[0].Body;
            var firstMessage = System.Text.Json.JsonSerializer.Deserialize<NewBookMessage>(firstMessageTemp);
            firstMessage.ReceiptHandle= result.Messages[0].ReceiptHandle;
            return firstMessage;
        }
        else
        {
            return null;
        }
    }


    public async Task DeleteMessage(string receiptHandle)
    {
        if (!_AwsSqsSettings.IsActive)
        {
            throw new Exception("Serviço aws está desabilitado no appsettings.");
        }

        var deleteMessageRequest = new DeleteMessageRequest();

        deleteMessageRequest.QueueUrl = _AwsSqsSettings.QueueBaseUrl;
        deleteMessageRequest.ReceiptHandle = receiptHandle + "aaa";

        await _amazonSQSClient.DeleteMessageAsync(deleteMessageRequest);
    }

}


        

