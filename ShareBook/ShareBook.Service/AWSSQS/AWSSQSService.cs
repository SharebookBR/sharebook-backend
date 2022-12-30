using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;
using ShareBook.Domain;
using ShareBook.Service.AWSSQS.Dto;
using System;
using System.Threading.Tasks;

namespace ShareBook.Service.AWSSQS
{
    public class AWSSQSService : IAWSSQSService
    {
        private readonly AWSSQSSettings _AWSSQSSettings;
        private readonly AmazonSQSClient _amazonSQSClient;

        public AWSSQSService(IOptions<AWSSQSSettings> AWSSQSSettings)
        {
            _AWSSQSSettings = AWSSQSSettings.Value;

            if (_AWSSQSSettings.IsActive)
            {
                // usando padrão Reflection
                var region = (Amazon.RegionEndpoint)typeof(Amazon.RegionEndpoint).GetField(_AWSSQSSettings.Region).GetValue(null);

                var awsCreds = new BasicAWSCredentials(AWSSQSSettings.Value.AccessKey, AWSSQSSettings.Value.SecretKey);
                _amazonSQSClient = new AmazonSQSClient(awsCreds, region);
            }
        }

        public async Task DeleteNewBookNotifyFromAWSSQSAsync(string receiptHandle)
        {
            if (!_AWSSQSSettings.IsActive)
            {
                throw new Exception("Serviço aws está desabilitado no appsettings.");
            }

            var deleteMessageRequest = new DeleteMessageRequest();

            deleteMessageRequest.QueueUrl = _AWSSQSSettings.QueueBaseUrl;
            deleteMessageRequest.ReceiptHandle = receiptHandle + "aaa";

            await _amazonSQSClient.DeleteMessageAsync(deleteMessageRequest);
        }

        public async Task<AWSSQSMessageNewBookNotifyResponse> GetNewBookNotifyFromAWSSQSAsync()
        {
            if (!_AWSSQSSettings.IsActive)
            {
                throw new Exception("Serviço aws está desabilitado no appsettings.");
            }

            var receiveMessageRequest = new ReceiveMessageRequest(_AWSSQSSettings.QueueBaseUrl);

            var result = await _amazonSQSClient.ReceiveMessageAsync(receiveMessageRequest);

            if (result.Messages.Count > 0)
            {
                var firstMessageTemp = result.Messages[0].Body;
                var firstMessage = System.Text.Json.JsonSerializer.Deserialize<AWSSQSMessageNewBookNotifyResponse>(firstMessageTemp);
                firstMessage.ReceiptHandle = result.Messages[0].ReceiptHandle;
                return firstMessage;
            }
            else
            {
                return null;
            }
        }

        public async Task SendNewBookNotifyToAWSSQSAsync(SendEmailRequest message)
        {
            if (_AWSSQSSettings.IsActive)
            {
                var request = new SendMessageRequest
                {
                    DelaySeconds = (int)TimeSpan.FromSeconds(5).TotalSeconds,
                    MessageBody = System.Text.Json.JsonSerializer.Serialize(message),
                    QueueUrl = _AWSSQSSettings.QueueBaseUrl
                };

                await _amazonSQSClient.SendMessageAsync(request);
            }
        }

        public async Task NotifyBookApproved(Book book)
        {
            if (!_AWSSQSSettings.IsActive)
                return;
 
            var message = new GetInterestedUsersRequest{
                BookId = book.Id,
                BookTitle = book.Title,
                CategoryId = book.CategoryId
            };
            
            var request = new SendMessageRequest
            {
                DelaySeconds = (int)TimeSpan.FromSeconds(5).TotalSeconds,
                MessageBody = System.Text.Json.JsonSerializer.Serialize(message),
                QueueUrl = $"{_AWSSQSSettings.QueueBaseUrl}/{_AWSSQSSettings.NewBookQueue}"
            };

            await _amazonSQSClient.SendMessageAsync(request);
        }
    }
}