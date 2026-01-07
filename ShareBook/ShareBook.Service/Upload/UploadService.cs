using Microsoft.Extensions.Options;
using ShareBook.Helper.Image;
using ShareBook.Service.Server;
using System;
using System.IO;
using System.Threading.Tasks;

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
            return ImageHelper.GenerateImageUrl(imageName, dinamicDirectory, _serverSettings.BackendUrl);
        }


        public async Task<string> UploadImageAsync(byte[] imageBytes, string imageName, string lastDirectory)
        {
            var dinamicDirectory = Path.Combine(_imageSettings.ImagePath, lastDirectory);

            await UploadFileAsync(imageBytes, imageName, dinamicDirectory);

            return GetImageUrl(imageName, lastDirectory);
        }

        public async Task<string> UploadPdfAsync(byte[] imageBytes, string imageName, string lastDirectory)
        {
            var dinamicDirectory = Path.Combine(_imageSettings.EBookPdfPath, lastDirectory);

            await UploadFileAsync(imageBytes, imageName, dinamicDirectory);

            return Path.Combine(lastDirectory, dinamicDirectory.Replace("wwwroot", ""), imageName);

        }

        private static async Task UploadFileAsync(byte[] imageBytes, string imageName, string dinamicDirectory)
        {
            var directoryBase = AppDomain.CurrentDomain.BaseDirectory + dinamicDirectory;
            if (!Directory.Exists(directoryBase))
                Directory.CreateDirectory(directoryBase);

            var imageCompletePath = Path.Combine(directoryBase, imageName);
            await File.WriteAllBytesAsync(imageCompletePath, imageBytes);
        }
    }
}
