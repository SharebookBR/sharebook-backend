using System;
using System.Collections.Generic;
using System.Text;

namespace ShareBook.Helper.Image
{
    public static  class ImageHelper
    {
        public static string GetExtension(string image)
        {
            return image.Split(".")[1];
        }

        public static string FormatImageName(string name, string extension)
        {
            return string.Format("{0}.{1}", name, extension);
        }
    }
}
