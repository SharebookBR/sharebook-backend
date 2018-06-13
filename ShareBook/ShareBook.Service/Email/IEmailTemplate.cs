using System.Threading.Tasks;

namespace ShareBook.Service
{
    public interface IEmailTemplate
    {
        Task<string> GenerateHtmlFromTemplateAsync(string template, object model);
    }
}
