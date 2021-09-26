using System.Text;

namespace ShareBook.Helper.Extensions {
    public static class ByteExtensions {
        public static string ByteArrayToString(this byte[] encodedByte) {
            var sOutput = new StringBuilder(encodedByte.Length);
            for (var i = 0; i < encodedByte.Length - 1; i++) {
                sOutput.Append(encodedByte[i].ToString("X2"));
            }
            return sOutput.ToString();
        }
    }
}