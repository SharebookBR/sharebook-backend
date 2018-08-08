using System.Threading.Tasks;
using ShareBook.Domain;

namespace ShareBook.Service
{
    public class BookUserEmailService : IBookUsersEmailService
    {
        private const string BookRequestedTemplate = "BookRequestedTemplate";
        private const string BookDonatedTemplate = "BookDonatedTemplate";
        private const string BookRequestedTitle = "Um livro foi solicitado - Sharebook";


        private readonly IUserService _userService;
        private readonly IBookService _bookService;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplate _emailTemplate;

        public BookUserEmailService(IUserService userService, IBookService bookService, IEmailService emailService, IEmailTemplate emailTemplate)
        {
            _userService = userService;
            _bookService = bookService;
            _emailService = emailService;
            _emailTemplate = emailTemplate;
        }

        public async Task SendEmailBookDonated(BookUser bookUser)
        {
            var bookDonated = _bookService.Get(bookUser.BookId);

            var grantee = _userService.Get(bookUser.UserId);

            await SendEmailBookDonatedToGrantee(bookDonated, grantee);
        }

        public async Task SendEmailBookRequested(BookUser bookUser)
        {
            var bookRequested = _bookService.Get(bookUser.BookId);

            var requestingUser = _userService.Get(bookUser.UserId);

            var administrators = _userService.GetAllAdministrators();

            foreach (var admin in administrators)
                await SendEmailBookRequestedToAdministrator(bookRequested, requestingUser, admin);
        }

        private async Task SendEmailBookRequestedToAdministrator(Book bookRequested, User requestingUser, User admin)
        {
            var vm = new
            {
                Book = bookRequested,
                RequestingUser = requestingUser,
                Administrator = admin
            };

            var html = await _emailTemplate.GenerateHtmlFromTemplateAsync(BookRequestedTemplate, vm);
            _emailService.Send(admin.Email, admin.Name, html, BookRequestedTitle);
        }

        private async Task SendEmailBookDonatedToGrantee(Book bookDonated, User grantee)
        {
            var vm = new
            {
                Book = bookDonated,
                User = grantee
            };

            var html = await _emailTemplate.GenerateHtmlFromTemplateAsync(BookDonatedTemplate, vm);
        }
    }
}
