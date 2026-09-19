using System.IO;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Webp;
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

        public static string FormatThumbnailName(string imageName)
        {
            if (string.IsNullOrWhiteSpace(imageName))
            {
                return null;
            }

            return $"{Path.GetFileNameWithoutExtension(Path.GetFileName(imageName))}.webp";
        }

        public static byte[] CreateBookThumbnail(
            byte[] imageBytes,
            int maxWidth = 360,
            int maxHeight = 540,
            int quality = 78,
            float sharpenSigma = 0.8f)
        {
            if (imageBytes == null || imageBytes.Length == 0)
            {
                throw new ArgumentException("A imagem da capa é obrigatória.", nameof(imageBytes));
            }

            if (maxWidth <= 0 || maxHeight <= 0)
            {
                throw new ArgumentException("As dimensões máximas do thumbnail devem ser maiores que zero.");
            }

            using var image = SixLabors.ImageSharp.Image.Load(imageBytes);

            image.Mutate(context => context.AutoOrient());

            if (image.Width > maxWidth || image.Height > maxHeight)
            {
                image.Mutate(context => context
                    .Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Size = new Size(maxWidth, maxHeight),
                        Sampler = KnownResamplers.Lanczos3,
                        Compand = true
                    })
                    .GaussianSharpen(sharpenSigma));
            }

            image.Metadata.ExifProfile = null;
            image.Metadata.IccProfile = null;
            image.Metadata.XmpProfile = null;

            using var output = new MemoryStream();
            image.Save(output, new WebpEncoder
            {
                FileFormat = WebpFileFormatType.Lossy,
                Quality = quality
            });

            return output.ToArray();
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

            using var memoryStreamInput = new MemoryStream(imageBytes);
            IImageFormat imageFormat = SixLabors.ImageSharp.Image.DetectFormat(memoryStreamInput);
            memoryStreamInput.Position = 0;
            
            using SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(memoryStreamInput);

            var width = image.Width * scalefactor / 100;
            var height = image.Height * scalefactor / 100;

            image.Mutate(x => x.Resize(width, height));

            var memoryStream = new MemoryStream();
            
            image.Save(memoryStream, imageFormat);

            return memoryStream.ToArray();
        }
    }
}
