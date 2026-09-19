using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ShareBook.Helper.Extensions
{
    public static class StringExtension
    {
        public static string GenerateSlug(this string phrase)
        {
            string str = phrase.RemoveAccent().ToLower();
            // invalid chars           
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // convert multiple spaces into one space   
            str = Regex.Replace(str, @"\s+", " ").Trim();
            // cut and trim 
            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
            str = Regex.Replace(str, @"\s", "-"); // hyphens   
            return str;
        }

        public static string NextAvailableCopySlug(this string baseSlug, IEnumerable<string> existingSlugs)
        {
            var occupiedSlugs = new HashSet<string>(existingSlugs ?? Array.Empty<string>(), StringComparer.Ordinal);
            if (!occupiedSlugs.Contains(baseSlug))
                return baseSlug;

            for (var copyNumber = 1; copyNumber < int.MaxValue; copyNumber++)
            {
                var candidate = $"{baseSlug}_copy{copyNumber}";
                if (!occupiedSlugs.Contains(candidate))
                    return candidate;
            }

            throw new InvalidOperationException($"Não foi possível gerar um slug disponível para '{baseSlug}'.");
        }

        public static string RemoveAccent(this string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public static string ToNormalizedSearchText(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var normalized = Regex.Replace(text, @"(?i)c\+\+", "cplusplus");
            normalized = Regex.Replace(normalized, @"(?i)c#", "csharp");
            normalized = Regex.Replace(normalized, @"(?i)f#", "fsharp");
            normalized = Regex.Replace(normalized, @"(?i)\.net", "dotnet");

            normalized = normalized
                .RemoveAccent()
                .Replace("-", " ")
                .Replace("_", " ");

            normalized = Regex.Replace(normalized, @"[^\w\s]", " ");
            normalized = Regex.Replace(normalized, @"\s+", " ").Trim();

            return normalized.ToLowerInvariant();
        }


        public static string AddIncremental(this string text)
        {

            var number = text.Split("_copy").Length == 2 ? Convert.ToInt32(text.Split("_copy")[1]) + 1 : 1;

            var onlyText = text.Split("_copy").Length == 2 ? text.Split("_copy")[0] : text;

            return $"{onlyText}_copy{number}";
        }
    }
}
