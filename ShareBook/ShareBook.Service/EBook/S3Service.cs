using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Options;
using System.IO;
using System.Threading.Tasks;

namespace ShareBook.Service.EBook
{
    public class S3Service : IS3Service
    {
        private readonly EBookStorageSettings _settings;

        public S3Service(IOptions<EBookStorageSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task<string> UploadAsync(Stream content, string key, string contentType)
        {
            var region = RegionEndpoint.GetBySystemName(_settings.S3Region);

            AmazonS3Client client;

            if (!string.IsNullOrEmpty(_settings.S3AccessKey) && !string.IsNullOrEmpty(_settings.S3SecretKey))
            {
                var credentials = new BasicAWSCredentials(_settings.S3AccessKey, _settings.S3SecretKey);
                client = new AmazonS3Client(credentials, region);
            }
            else
            {
                // Usa credenciais padrão do ambiente: IAM role, variáveis AWS_* ou ~/.aws/credentials
                client = new AmazonS3Client(region);
            }

            using (client)
            {
                var transferUtility = new TransferUtility(client);

                var uploadRequest = new TransferUtilityUploadRequest
                {
                    BucketName = _settings.S3BucketName,
                    Key = key,
                    InputStream = content,
                    ContentType = contentType,
                    CannedACL = S3CannedACL.PublicRead,
                };

                await transferUtility.UploadAsync(uploadRequest);
            }

            return $"https://{_settings.S3BucketName}.s3.{_settings.S3Region}.amazonaws.com/{key}";
        }
    }
}
