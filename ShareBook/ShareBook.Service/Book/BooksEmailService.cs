using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ShareBook.Domain;
using ShareBook.Domain.DTOs;
using ShareBook.Service.AwsSqs;
using ShareBook.Service.Server;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public class BooksEmailService : IBooksEmailService
    {
        private const string NewBookInsertedTemplate = "NewBookInsertedTemplate";
        private const string NewBookInsertedTitle = "Novo livro incluído - Sharebook";
        private const string WaitingApprovalTemplate = "WaitingApprovalTemplate";
        private const string EbookWaitingApprovalTemplate = "EbookWaitingApprovalTemplate";
        private const string WaitingApprovalTitle = "Aguarde aprovação do livro - Sharebook";
        private const string BookApprovedTemplate = "BookApprovedTemplate";
        private const string EbookApprovedTemplate = "EbookApprovedTemplate";
        private const string BookApprovedTitle = "Livro aprovado - Sharebook";
        private const string NewBookNotifyTemplate = "NewBookNotifyTemplate";
        private const string BookReceivedTemplate = "BookReceivedTemplate";

        private readonly IEmailService _emailService;
        private readonly IUserService _userService;
        private readonly IEmailTemplate _emailTemplate;

        public BooksEmailService(
            IEmailService emailService,
            IUserService userService,
            IEmailTemplate emailTemplate,
            IOptions<ServerSettings> serverSettings,
            IConfiguration configuration,
            MailSenderHighPriorityQueue mailSenderHighPriorityQueue)
        {
            _emailService = emailService;
            _userService = userService;
            _emailTemplate = emailTemplate;
        }

        public async Task SendEmailBookApprovedAsync(Book book)
        {
            if (book.User == null)
                book.User = await _userService.FindAsync(book.UserId);

            if (book.User.AllowSendingEmail)
            {
                var vm = new
                {
                    Book = book,
                    book.User,
                    ChooseDate = book.ChooseDate?.ToString("dd/MM/yyyy")
                };
                var templateName = book.IsEbook() ? EbookApprovedTemplate : BookApprovedTemplate;
                var html = await _emailTemplate.GenerateHtmlFromTemplateAsync(templateName, vm);
                await _emailService.SendAsync(book.User.Email, book.User.Name, html, BookApprovedTitle, copyAdmins: true, highPriority: true);
            }
        }

        public async Task SendEmailBookReceivedAsync(Book book)
        {
            if (book.User == null)
                book.User = await _userService.FindAsync(book.UserId);

            if (book.User.AllowSendingEmail)
            {
                var vm = new
                {
                    Book = book,
                    book.User,
                    WinnerName = book.WinnerName(),
                };

                var htmt = await _emailTemplate.GenerateHtmlFromTemplateAsync(BookReceivedTemplate, vm);
                await _emailService.SendAsync(book.User.Email, book.User.Name, htmt, BookReceivedTemplate, copyAdmins: true, highPriority: true);
            }
        }

        public async Task SendEmailNewBookInsertedAsync(Book book)
        {
            if (book.User == null)
                book.User = await _userService.FindAsync(book.UserId);

            var userStats = await _userService.GetStatsAsync(book.UserId);

            await SendEmailNewBookInsertedToAdministrators(book, userStats);

            if (book.User.AllowSendingEmail)
                await SendEmailWaitingApprovalToUser(book);
        }

        private async Task SendEmailNewBookInsertedToAdministrators(Book book, UserStatsDTO userStats)
        {
            var model = new
            {
                Book = book,
                UserStats = userStats
            };

            var html = await _emailTemplate.GenerateHtmlFromTemplateAsync(NewBookInsertedTemplate, model);
            await _emailService.SendToAdminsAsync(html, NewBookInsertedTitle);
        }

        private async Task SendEmailWaitingApprovalToUser(Book book)
        {
            if (book.User.AllowSendingEmail)
            {
                var templateName = book.IsEbook() ? EbookWaitingApprovalTemplate : WaitingApprovalTemplate;
                var html = await _emailTemplate.GenerateHtmlFromTemplateAsync(templateName, book);

                await _emailService.SendAsync(book.User.Email, book.User.Name, html, WaitingApprovalTitle, copyAdmins: false, highPriority: true);
            }
        }

        public async Task SendEmailCopyrightReportAsync(Book book)
        {
            var subject = $"[Direitos Autorais] Report de violação — {book.Title}";
            var body = $@"Um usuário reportou possível violação de direitos autorais no e-book abaixo.
<br/><br/>
<strong>Título:</strong> {book.Title}<br/>
<strong>Autor:</strong> {book.Author}<br/>
<strong>Slug:</strong> {book.Slug}<br/>
<br/>
Por favor, revise e, se necessário, oculte temporariamente o e-book no painel administrativo.";

            await _emailService.SendToAdminsAsync(body, subject);
        }
    }
}
