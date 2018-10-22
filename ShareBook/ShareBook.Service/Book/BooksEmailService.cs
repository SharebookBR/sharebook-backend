using ShareBook.Domain;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public class BooksEmailService : IBooksEmailService
    {
        private const string NewBookInsertedTemplate = "NewBookInsertedTemplate";
        private const string NewBookInsertedTitle = "Novo livro incluído - Sharebook";
        private const string WaitingApprovalTemplate = "WaitingApprovalTemplate";
        private const string WaitingApprovalTitle = "Aguarde aprovação do livro - Sharebook";
        private const string BookApprovedTemplate = "BookApprovedTemplate";
        private const string BookApprovedTitle = "Livro aprovado - Sharebook";

        private readonly IEmailService _emailService;
        private readonly IUserService _userService;
        private readonly IEmailTemplate _emailTemplate;
        public BooksEmailService(IEmailService emailService, IUserService userService, IEmailTemplate emailTemplate)
        {
            _emailService = emailService;
            _userService = userService;
            _emailTemplate = emailTemplate;
        }

        public async Task SendEmailBookApproved(Book book)
        {
            if (book.User == null)
                book.User = _userService.Find(book.UserId);

            var html = await _emailTemplate.GenerateHtmlFromTemplateAsync(BookApprovedTemplate, book);
            await _emailService.Send(book.User.Email, book.User.Name, html, BookApprovedTitle, true);
        }

        public async Task SendEmailNewBookInserted(Book book)
        {
            if (book.User == null)
                book.User = _userService.Find(book.UserId);

            await SendEmailNewBookInsertedToAdministrator(book);

            await SendEmailWaitingApprovalToUser(book);
        }

        private async Task SendEmailNewBookInsertedToAdministrator(Book book)
        {
            var html = await _emailTemplate.GenerateHtmlFromTemplateAsync(NewBookInsertedTemplate, book);
            await _emailService.SendToAdmins(html, NewBookInsertedTitle);
        }

        private async Task SendEmailWaitingApprovalToUser(Book book)
        {
            var html = await _emailTemplate.GenerateHtmlFromTemplateAsync(WaitingApprovalTemplate, book);
            await _emailService.Send(book.User.Email, book.User.Name, html, WaitingApprovalTitle, true);
        }
    }
}
