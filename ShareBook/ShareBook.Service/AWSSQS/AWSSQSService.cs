using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;
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

            var awsCreds = new BasicAWSCredentials(AWSSQSSettings.Value.AccessKey, AWSSQSSettings.Value.SecretKey);
            _amazonSQSClient = new AmazonSQSClient(awsCreds, Amazon.RegionEndpoint.SAEast1);
        }

        public async Task SendNewBookNotifyToAWSSQSAsync(AWSSQSMessageNewBookNotify message)
        {
            var request = new SendMessageRequest
            {
                DelaySeconds = (int)TimeSpan.FromSeconds(5).TotalSeconds,
                MessageBody = System.Text.Json.JsonSerializer.Serialize(message),
                QueueUrl = _AWSSQSSettings.QueueUrl
            };

            await _amazonSQSClient.SendMessageAsync(request);
        }
    }
}