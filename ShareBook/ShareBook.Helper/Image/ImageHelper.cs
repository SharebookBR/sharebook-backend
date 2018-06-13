using System.IO;

namespace ShareBook.Helper.Image
{
    public static class ImageHelper
    {
        public static string FormatImageName(string originalName, string newName)
            => originalName.Replace(Path.GetFileNameWithoutExtension(originalName), newName);
    }
}
