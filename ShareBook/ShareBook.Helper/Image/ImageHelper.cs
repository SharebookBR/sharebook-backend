using ShareBook.Helper.Extensions;
using System.IO;

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
    }
}
