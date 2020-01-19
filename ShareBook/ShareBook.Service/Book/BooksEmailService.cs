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
        private const string BookSentTemplate = "BookSentTemplate";
        private const string BookReceivedTemplate = "BookReceivedTemplate";
        private const string BookReceivedTitle = "Livro entregue";
        private const string BookTrackingNumberNoticeWinnerTitle = "Seu livro foi postado - Sharebook";

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

            if (book.User.AllowSendingEmail)
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
        }

        public async Task SendEmailNewBookInserted(Book book)
        {
            if (book.User == null)
                book.User = _userService.Find(book.UserId);

            await SendEmailNewBookInsertedToAdministrators(book);

            if (book.User.AllowSendingEmail)
                await SendEmailWaitingApprovalToUser(book);
        }

        private async Task SendEmailNewBookInsertedToAdministrators(Book book)
        {
            var html = await _emailTemplate.GenerateHtmlFromTemplateAsync(NewBookInsertedTemplate, book);
            await _emailService.SendToAdmins(html, NewBookInsertedTitle);
        }

        private async Task SendEmailWaitingApprovalToUser(Book book)
        {
            if (book.User.AllowSendingEmail)
            {
                var html = await _emailTemplate.GenerateHtmlFromTemplateAsync(WaitingApprovalTemplate, book);

                await _emailService.Send(book.User.Email, book.User.Name, html, WaitingApprovalTitle, copyAdmins: true);
            }
        }

        public async Task SendEmailBookSent(BookUser bookUserWinner, Book book)
        {
            if (bookUserWinner.User.AllowSendingEmail)
            {
                var vm = new
                {
                    book = book,
                    NameFacilitator = book.UserFacilitator.Name,
                    LinkedInFacilitator = book.UserFacilitator.Linkedin,
                    ZapFacilitator = book.UserFacilitator.Phone,
                    EmailFacilitator = book.UserFacilitator.Email,
                };
                var html = await _emailTemplate.GenerateHtmlFromTemplateAsync(BookSentTemplate, vm);
                await _emailService.Send(bookUserWinner.User.Email, bookUserWinner.User.Name, html, BookTrackingNumberNoticeWinnerTitle, copyAdmins: false);
            }
        }

        public async Task SendEmailBookReceived(Book book)
        {
            if (book.User.AllowSendingEmail)
            {
                var vm = new
                {
                    book = book,
                };
                var html = await _emailTemplate.GenerateHtmlFromTemplateAsync(BookReceivedTemplate, vm);
                await _emailService.Send(book.User.Email, book.User.Name, html, BookReceivedTitle, copyAdmins: false);
            }
        }
    }
}
