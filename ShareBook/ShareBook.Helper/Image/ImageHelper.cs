using System.IO;

namespace ShareBook.Helper.Image
{
    public static class ImageHelper
    {
        public static string FormatImageName(string originalName, string newName)
        {
            var newFileName = originalName.Replace(Path.GetFileNameWithoutExtension(originalName), newName);

            return Path.GetFileName(newFileName);

        }
    }
}
