using Microsoft.Extensions.Options;
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
         
        public string UploadImage(byte[] imageBytes, string imageName)
        {
            _imageSettings.Directory = Path.Combine(_imageSettings.Directory, "Books");
            if (!Directory.Exists(_imageSettings.Directory))
                Directory.CreateDirectory(_imageSettings.Directory);                  

            var imagePath = Path.Combine(_imageSettings.Directory, imageName);
            File.WriteAllBytes(imagePath, imageBytes);
            return imagePath;
        }
    }
}
