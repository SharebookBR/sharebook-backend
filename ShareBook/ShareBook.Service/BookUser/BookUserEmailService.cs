using System.Threading.Tasks;
using ShareBook.Domain.Entities;

namespace ShareBook.Service
{
    public class BookUserEmailService : IBookUsersEmailService
    {
        private const string BookRequestedTemplate = "BookRequestedTemplate";
        private const string BookDonatedTemplate = "BookDonatedTemplate";
        private const string BookDonatedTitle = "Parabéns você foi selecionado!";
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
            var bookDonated = bookUser.Book;
            bookDonated.User = _userService.Get(bookUser.Book.UserId);

            var grantee = bookUser.User;

            await SendEmailBookDonatedToGrantee(bookDonated, grantee);
        }

        public async Task SendEmailBookRequested(BookUser bookUser)
        {
            // TODO - Ajustar para apenas um SELECT 
            var bookRequested = _bookService.Get(bookUser.BookId);

            bookRequested.User = _userService.Get(bookRequested.UserId);

            var requestingUser = _userService.Get(bookUser.UserId);

            var administrators = _userService.GetAllAdministrators();

            foreach (var admin in administrators)
                await SendEmailBookRequestedToAdministrator(bookUser, bookRequested, requestingUser, admin);
        }

        private async Task SendEmailBookRequestedToAdministrator(BookUser request, Book bookRequested, User requestingUser, User admin)
        {
            var vm = new
            {
                Request = request,
                Book = bookRequested,
                RequestingUser = requestingUser,
                Administrator = admin
            };

            var html = await _emailTemplate.GenerateHtmlFromTemplateAsync(BookRequestedTemplate, vm);
            bool copyAdmins = false;
            _emailService.Send(admin.Email, admin.Name, html, BookRequestedTitle, copyAdmins);
        }

        private async Task SendEmailBookDonatedToGrantee(Book bookDonated, User grantee)
        {
            var vm = new
            {
                Book = bookDonated,
                User = grantee
            };
            bool copyAdmins = true;
            var html = await _emailTemplate.GenerateHtmlFromTemplateAsync(BookDonatedTemplate, vm);
            _emailService.Send(grantee.Email, grantee.Name, html, BookDonatedTitle, copyAdmins);
        }
    }
}
