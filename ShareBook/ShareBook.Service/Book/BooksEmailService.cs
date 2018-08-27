using System.Threading.Tasks;
using ShareBook.Domain;

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
                book.User = _userService.Get(book.UserId);

            var vm = new
            {
                Book = book
            };

            bool copyAdmins = true;
            var html = await _emailTemplate.GenerateHtmlFromTemplateAsync(BookApprovedTemplate, vm);
            _emailService.Send(book.User.Email, book.User.Name, html, BookApprovedTitle, copyAdmins);
        }

        public async Task SendEmailNewBookInserted(Book book)
        {
            if (book.User == null)
                book.User = _userService.Get(book.UserId);

            var administrators = _userService.GetAllAdministrators();

            foreach (var admin in administrators)
                await SendEmailNewBookInsertedToAdministrator(book, admin);

            await SendEmailWaitingApprovalToUser(book);

        }

        private async Task SendEmailNewBookInsertedToAdministrator(Book book, User administrator)
        {
            var vm = new
            {
                Book = book,
                Administrator = administrator
            };

            var html = await _emailTemplate.GenerateHtmlFromTemplateAsync(NewBookInsertedTemplate, vm);
            bool copyAdmins = false;
            _emailService.Send(administrator.Email, administrator.Name, html, NewBookInsertedTitle, copyAdmins);
        }

        private async Task SendEmailWaitingApprovalToUser(Book book)
        {
            var vm = new
            {
                Book = book
            };
            bool copyAdmins = true;
            var html = await _emailTemplate.GenerateHtmlFromTemplateAsync(WaitingApprovalTemplate, vm);
            _emailService.Send(book.User.Email, book.User.Name, html, WaitingApprovalTitle, copyAdmins);
        }
    }
}
