using ShareBook.Domain;
using System.Threading.Tasks;

namespace ShareBook.Service;

public interface IBooksEmailService
{
    Task SendEmailNewBookInsertedAsync(Book book);

    Task SendEmailBookApprovedAsync(Book book);

    Task SendEmailBookReceivedAsync(Book book);

    Task SendEmailCopyrightReportAsync(Book book);

    Task SendEmailBookReleasedAsync(Book book, User user, string coverUrl, string frontendUrl, string unsubscribeUrl);
}