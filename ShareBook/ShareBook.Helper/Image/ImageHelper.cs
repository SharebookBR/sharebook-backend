using ShareBook.Helper.Extensions;
using System.IO;

namespace ShareBook.Helper.Image
{
    public static class ImageHelper
    {
        public static string FormatImageName(string originalName, string newName)
        {
            var newFileName = originalName.Replace(Path.GetFileNameWithoutExtension(originalName), newName.GenerateSlug());

            return Path.GetFileName(newFileName);
        }

        public static string GetImageUrl(string imageName, string directory, string serverUrl)
        {
            return serverUrl + directory.Replace("wwwroot", "") + "/" + imageName;
        }
    }
}
