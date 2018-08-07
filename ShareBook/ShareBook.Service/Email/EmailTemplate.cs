using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public class EmailTemplate : IEmailTemplate
    {
        private string TemplatesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Email","Templates","{0}.html");
        const string PropertyRegex = @"\{(.*?)\}";
        private Dictionary<string, string> Templates { get; set; } = new Dictionary<string, string>();

        private string GetPropValue(object obj, string propName)
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

        private async Task<string> GetTemplateAsync(string template)
        {
            if (!Templates.ContainsKey(template))
            {
                var templatePath = string.Format(TemplatesFolder, template);
                Templates.Add(template, await File.ReadAllTextAsync(templatePath));
            }
            return Templates[template];
        }

        public async Task<string> GenerateHtmlFromTemplateAsync(string template, object model)
        {
            var templateString = await GetTemplateAsync(template);
            var matches = Regex.Matches(templateString, PropertyRegex);
            foreach (Match item in matches)
            {
                templateString = templateString.Replace(item.Value, GetPropValue(model, item.Groups[1].Value));
            }
            return templateString;
        }
    }
}
