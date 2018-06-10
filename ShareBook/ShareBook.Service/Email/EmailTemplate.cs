using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    //TODO: transformar de static para singleton
    public static class EmailTemplate
    {
        const string TemplatesFolder = "Email\\Templates\\{0}.html";
        const string PropertyRegex = @"\{(.*?)\}";
        private static Dictionary<string, string> Templates { get; set; } = new Dictionary<string, string>();

        private static async Task<string> GetTemplate(string template)
        {
            if (!Templates.ContainsKey(template))
            {
                var templatePath = string.Format(TemplatesFolder, template);
                Templates.Add(template, await File.ReadAllTextAsync(templatePath));
            }
            return Templates[template];
        }

        internal static async Task<string> GenerateHtmlFromTemplate(string template, object model)
        {
            var templateString = await GetTemplate(template);
            var matches = Regex.Matches(templateString, PropertyRegex);
            foreach (Match item in matches)
            {
                templateString = templateString.Replace(item.Value, model.GetPropValue(item.Groups[1].Value));
            }
            return templateString;
        }

        static string GetPropValue(this object obj, string propName)
        {
            string[] nameParts = propName.Split('.');
            if (nameParts.Length == 1)
            {
                return obj.GetType().GetProperty(propName).GetValue(obj, null)?.ToString();
            }

            foreach (string part in nameParts)
            {
                if (obj == null) { return null; }

                Type type = obj.GetType();
                PropertyInfo info = type.GetProperty(part);
                if (info == null) { return null; }

                obj = info.GetValue(obj, null);
            }
            return obj?.ToString();
        }
    }
}
