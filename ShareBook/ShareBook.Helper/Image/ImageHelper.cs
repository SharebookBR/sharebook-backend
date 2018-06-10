using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ShareBook.Helper.Image
{
    public static class ImageHelper
    {
        public static string FormatImageName(string originalName, string newName)
        {
            return originalName.Replace(Path.GetFileNameWithoutExtension(originalName), newName);
        }

    }
}
