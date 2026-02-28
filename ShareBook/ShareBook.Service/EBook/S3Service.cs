using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ShareBook.Service.EBook
{
    public class S3Service : IS3Service
    {
        private readonly AwsS3Settings _settings;

        public S3Service(IOptions<AwsS3Settings> settings)
        {
            _settings = settings.Value;
        }

        public async Task<string> UploadAsync(Stream content, string key, string contentType)
        {
            using var client = CreateClient();
            var transferUtility = new TransferUtility(client);

            var uploadRequest = new TransferUtilityUploadRequest
            {
                BucketName = _settings.S3BucketName,
                Key = key,
                InputStream = content,
                ContentType = contentType
            };

            await transferUtility.UploadAsync(uploadRequest);

            return key;
        }

        public Task<string> GeneratePreSignedDownloadUrlAsync(string key, string fileName)
        {
            using var client = CreateClient();

            var expiresInMinutes = _settings.DownloadUrlExpirationMinutes <= 0
                ? 5
                : _settings.DownloadUrlExpirationMinutes;

            var request = new GetPreSignedUrlRequest
            {
                BucketName = _settings.S3BucketName,
                Key = key,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddMinutes(expiresInMinutes)
            };

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                request.ResponseHeaderOverrides.ContentDisposition = $"inline; filename=\"{fileName}\"";
            }

            request.ResponseHeaderOverrides.ContentType = "application/pdf";

            var preSignedUrl = client.GetPreSignedURL(request);
            return Task.FromResult(preSignedUrl);
        }

        private AmazonS3Client CreateClient()
        {
            var region = RegionEndpoint.GetBySystemName(_settings.S3Region);

            if (!string.IsNullOrEmpty(_settings.S3AccessKey) && !string.IsNullOrEmpty(_settings.S3SecretKey))
            {
                var credentials = new BasicAWSCredentials(_settings.S3AccessKey, _settings.S3SecretKey);
                return new AmazonS3Client(credentials, region);
            }

            // Usa credenciais padrão do ambiente: IAM role, variáveis AWS_* ou ~/.aws/credentials
            return new AmazonS3Client(region);
        }
    }
}
