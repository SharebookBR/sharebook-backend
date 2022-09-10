using Microsoft.Extensions.Options;
using ShareBook.Helper.Image;
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

        public string GetImageUrl(string imageName, string lastDirectory)
        {
            var dinamicDirectory = _imageSettings.ImagePath + "/" + lastDirectory;
            return ImageHelper.GenerateImageUrl(imageName, dinamicDirectory, _serverSettings.DefaultUrl);
        }


        public string UploadImage(byte[] imageBytes, string imageName, string lastDirectory)
        {
            var dinamicDirectory = Path.Combine(_imageSettings.ImagePath, lastDirectory);

            var directoryBase = AppDomain.CurrentDomain.BaseDirectory + dinamicDirectory;

            var testPath = Path.Combine(directoryBase, imageName);

            if (File.Exists(testPath))
            {
                var extension = Path.GetExtension(imageName);
                imageName = Path.GetFileNameWithoutExtension(testPath) + DateTimeOffset.Now.ToUnixTimeSeconds() + extension;
            }

            UploadFile(imageBytes, imageName, dinamicDirectory);

            return GetImageUrl(imageName, lastDirectory);
        }
     
        public string UploadPdf(byte[] imageBytes, string imageName, string lastDirectory)
        {
            var dinamicDirectory = Path.Combine(_imageSettings.EBookPdfPath, lastDirectory);

            UploadFile(imageBytes, imageName, dinamicDirectory);

            return Path.Combine(lastDirectory, dinamicDirectory.Replace("wwwroot", ""), imageName);

        }

        private static void UploadFile(byte[] imageBytes, string imageName, string dinamicDirectory)
        {
            var directoryBase = AppDomain.CurrentDomain.BaseDirectory + dinamicDirectory;
            if (!Directory.Exists(directoryBase))
                Directory.CreateDirectory(directoryBase);

            var imageCompletePath = Path.Combine(directoryBase, imageName);
            File.WriteAllBytes(imageCompletePath, imageBytes);
        }
    }
}
