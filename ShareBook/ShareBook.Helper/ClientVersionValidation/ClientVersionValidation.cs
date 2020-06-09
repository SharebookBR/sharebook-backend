using System;
using System.Text.RegularExpressions;

namespace ShareBook.Helper
{
    static public class ClientVersionValidation
    {
        static public bool IsValidVersion(string version, string minVersion)
        {
            try
            {
                (int majorMin, int minorMin, int patchMin) = VersionDeconstructor(minVersion);
                (int major, int minor, int patch) = VersionDeconstructor(version);

                if (major < majorMin) return false;
                if (major > majorMin) return true;

                if (minor < minorMin) return false;
                if (minor > minorMin) return true;

                if (patch < patchMin) return false;
                else return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        static private Tuple<int, int, int> VersionDeconstructor(string version)
        {
            string pattern = @"v([0-9]{1,2})\.([0-9]{1,2})\.([0-9]{1,2})$";
            Regex rg = new Regex(pattern);

            MatchCollection matches = rg.Matches(version);

            if (matches.Count != 1) throw new Exception("Formato inválido");

            var major = int.Parse(matches[0].Groups[1].Value);
            var minor = int.Parse(matches[0].Groups[2].Value);
            var patch = int.Parse(matches[0].Groups[3].Value);

            return Tuple.Create(major, minor, patch);
        }
    }
}
