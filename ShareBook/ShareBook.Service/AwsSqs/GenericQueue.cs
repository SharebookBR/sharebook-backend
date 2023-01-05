using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShareBook.Domain.Exceptions;
using ShareBook.Service.AwsSqs.Dto;
using System;
using System.Threading.Tasks;

namespace ShareBook.Service.AwsSqs;

public class GenericQueue<T> : IAwsSqsQueue<T>
{
    protected AwsSqsSettings _awsSqsSettings;
    protected AmazonSQSClient _amazonSQSClient;

    protected string _queueUrl;

    // protected readonly ILogger _logger;

    public GenericQueue(IOptions<AwsSqsSettings> awsSqsSettings)
    {
        _awsSqsSettings = awsSqsSettings?.Value;
        bool isActive = _awsSqsSettings?.IsActive ?? false;

        if (isActive)
        {
            // usando padrão Reflection
            var region = (Amazon.RegionEndpoint)typeof(Amazon.RegionEndpoint).GetField(_awsSqsSettings.Region).GetValue(null);

            var awsCreds = new BasicAWSCredentials(awsSqsSettings.Value.AccessKey, awsSqsSettings.Value.SecretKey);
            _amazonSQSClient = new AmazonSQSClient(awsCreds, region);
        }
    }

    public async Task SendMessage(T message)
    {
        if (!_awsSqsSettings.IsActive)
        {
            // _logger.LogInformation("Serviço aws sqs está desabilitado no appsettings.");
            return;
        }
            
        var request = new SendMessageRequest
        {
            DelaySeconds = (int)TimeSpan.FromSeconds(5).TotalSeconds,
            MessageBody = System.Text.Json.JsonSerializer.Serialize(message),
            QueueUrl = _queueUrl
        };

        await _amazonSQSClient.SendMessageAsync(request);
    }

    public async Task<SharebookMessage<T>> GetMessage()
    {
        if (!_awsSqsSettings.IsActive)
        {
            throw new AwsSqsDisabledException("Serviço aws sqs está desabilitado no appsettings.");
        }

        var receiveMessageRequest = new ReceiveMessageRequest(_queueUrl);

        var result = await _amazonSQSClient.ReceiveMessageAsync(receiveMessageRequest);

        if (result.Messages.Count > 0)
        {
            var firstMessageTemp = result.Messages[0].Body;
            var firstMessage = System.Text.Json.JsonSerializer.Deserialize<T>(firstMessageTemp);

            var envelope = new SharebookMessage<T>();
            envelope.Body = firstMessage;
            envelope.ReceiptHandle = result.Messages[0].ReceiptHandle;

            return envelope;
        }
        else
        {
            return null;
        }
    }


    public async Task DeleteMessage(string receiptHandle)
    {
        if (!_awsSqsSettings.IsActive)
        {
            throw new AwsSqsDisabledException("Serviço aws sqs está desabilitado no appsettings.");
        }

        var deleteMessageRequest = new DeleteMessageRequest();

        deleteMessageRequest.QueueUrl = _queueUrl;
        deleteMessageRequest.ReceiptHandle = receiptHandle + "aaa";

        await _amazonSQSClient.DeleteMessageAsync(deleteMessageRequest);
    }

}


        

