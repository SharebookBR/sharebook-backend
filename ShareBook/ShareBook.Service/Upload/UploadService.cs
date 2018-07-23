using Microsoft.Extensions.Options;
using ShareBook.Service.Server;
using System;
using System.IO;


namespace ShareBook.Service.Upload
{
    public class UploadService : IUploadService
    {
        private readonly ImageSettings _imageSettings;
        private readonly ServerSettings _serverSettings;

        public UploadService(IOptions<ImageSettings> imageSettings, IOptions<ServerSettings> serverSettings)
        {
            _imageSettings = imageSettings.Value;
            _serverSettings = serverSettings.Value;
        }
         
        public string UploadImage(byte[] imageBytes, string imageName)
        {
            var directory = AppDomain.CurrentDomain.BaseDirectory + _imageSettings.ImagePath;
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);                  

            var imageCompletePath = Path.Combine(directory, imageName);
            File.WriteAllBytes(imageCompletePath, imageBytes);

            return GetImageUrl(imageName);
        }

        public string GetImageUrl(string imageName)
        {
            return _serverSettings.DefaultUrl + _imageSettings.ImagePath.Replace("wwwroot", "") + "/" + imageName;
        }
    }
}
