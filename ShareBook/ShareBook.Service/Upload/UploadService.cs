using Microsoft.Extensions.Options;
using ShareBook.Helper.Image;
using ShareBook.Service.Server;
using System;
using System.IO;


namespace ShareBook.Service.Upload
{
    public class UploadService
    {
        private readonly ImageSettings _imageSettings;
        private readonly ServerSettings _serverSettings;

        public UploadService(IOptions<ImageSettings> imageSettings, IOptions<ServerSettings> serverSettings)
        {
            _imageSettings = imageSettings.Value;
            _serverSettings = serverSettings.Value;
        }
         
        public string UploadImage(byte[] imageBytes, string imageName, string lastDirectory)
        {
            var dinamicDirectory = Path.Combine(_imageSettings.ImagePath, lastDirectory);

            var directoryBase = AppDomain.CurrentDomain.BaseDirectory + dinamicDirectory;
            if (!Directory.Exists(directoryBase))
                Directory.CreateDirectory(directoryBase);                  

            var imageCompletePath = Path.Combine(directoryBase, imageName);
            File.WriteAllBytes(imageCompletePath, imageBytes);

            return GetImageUrl(imageName, lastDirectory);
        }
        
        public string GetImageUrl(string imageName, string lastDirectory)
        {
            var dinamicDirectory = _imageSettings.ImagePath + "/" + lastDirectory;
            return ImageHelper.GenerateImageUrl(imageName, dinamicDirectory, _serverSettings.DefaultUrl);
        }
    }
}
