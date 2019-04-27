using Google.Cloud.Storage.V1;
using ShareBook.Domain.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ShareBook.Service.Upload
{
    public class UploadImageGoogleCloudStorage<T>: IUploadService<T>
    {
        private readonly string _bucketName;
        private readonly StorageClient _storageClient;

        public UploadImageGoogleCloudStorage(string bucketName)
        {
            _bucketName = bucketName;
            _storageClient = StorageClient.Create();
        }

        public string GetUrl(string imageName)
        {
            try
            {
                return _storageClient.GetObject(_bucketName, imageName)?.MediaLink;
            }
            catch (Google.GoogleApiException ex)
            {
                return string.Empty;
            }
            
        }

        public string Upload(byte[] imageBytes, string imageName)
        {
            try
            {
                var imageAcl = PredefinedObjectAcl.PublicRead;

                var imageObject = _storageClient.UploadObject(
                    bucket: _bucketName,
                    objectName: imageName,
                    contentType: "image/jpeg",
                    source: new MemoryStream(imageBytes),
                    options: new UploadObjectOptions { PredefinedAcl = imageAcl }
                );

                return imageObject.MediaLink;
            }
            catch (Google.GoogleApiException ex)
            {
                throw ex;
            }           
        }
    }
}
