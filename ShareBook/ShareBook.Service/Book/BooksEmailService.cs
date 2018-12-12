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
        private readonly IEmailTemplate _emailTemplate;

        public BooksEmailService(IEmailService emailService, IEmailTemplate emailTemplate)
        {
            _emailService = emailService;
            _emailTemplate = emailTemplate;
        }

        public async Task SendEmailBookApproved(Book book)
        {
            var vm = new
            {
                Book = book,
                book.User,
                ChooseDate = book.ChooseDate?.ToString("dd/MM/yyyy")
            };

            var html = await _emailTemplate.GenerateHtmlFromTemplateAsync(BookApprovedTemplate, vm);
            await _emailService.Send(book.User.Email, book.User.Name, html, BookApprovedTitle, true);
        }

        public async Task SendEmailNewBookInserted(Book book)
        {
            await SendEmailNewBookInsertedToAdministrators(book);

            await SendEmailWaitingApprovalToUser(book);
        }

        private async Task SendEmailNewBookInsertedToAdministrators(Book book)
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
