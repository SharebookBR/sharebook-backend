using Microsoft.Extensions.Options;
using System;
using System.IO;


namespace ShareBook.Service.Upload
{
    public class UploadService : IUploadService
    {
        private readonly ImageSettings _imageSettings;

        public UploadService(IOptions<ImageSettings> imageSettings)
        {
            _imageSettings = imageSettings.Value;
        }

        public void UploadImage(byte[] imageBytes, string imageName)
        {
            var directory = Path.Combine(_imageSettings.Directory, DateTime.UtcNow.Year.ToString() + DateTime.UtcNow.Month.ToString()); 
            
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);                  

            var imagePath = Path.Combine(directory, imageName);
            File.WriteAllBytes(imagePath, imageBytes);
        }
    }
}
