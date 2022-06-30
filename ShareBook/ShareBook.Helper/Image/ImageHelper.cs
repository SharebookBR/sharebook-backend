using System.IO;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp;
using System;

namespace ShareBook.Helper.Image
{
    public static class ImageHelper
    {
        public static string FormatImageName(string originalName, string slug)
        {
            var newFileName = originalName.Replace(Path.GetFileNameWithoutExtension(originalName), slug);

            return Path.GetFileName(newFileName);
        }

        public static string GenerateImageUrl(string imageName, string directory, string serverUrl)
        {
            return serverUrl + directory.Replace("wwwroot", "") + "/" + imageName;
        }
        
        /// <summary>
        /// Scale an image by a scale factor
        /// </summary>
        /// <param name="imageBytes">The image bytes</param>
        /// <param name="scalefactor">The percentage to increase (>100) or decrease(<100) the size of the image</param>
        /// <returns>The resized image as a byte[]</returns>
        /// <exception cref="ArgumentException"></exception>
        public static byte[] ResizeImage(byte[] imageBytes, int scalefactor)
        {
            if (scalefactor <= 0)
            {
                throw new ArgumentException("'scalefactor' deve ser maior que 0");
            }

            SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(imageBytes, out IImageFormat imageFormat);

            var width = image.Width * scalefactor / 100;
            var height = image.Height * scalefactor / 100;

            image.Mutate(x => x.Resize(width, height));

            var memoryStream = new MemoryStream();
            
            image.Save(memoryStream, imageFormat);

            return memoryStream.ToArray();
        }
    }
}
