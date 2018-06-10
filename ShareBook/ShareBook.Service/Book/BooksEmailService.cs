using System.Threading.Tasks;
using ShareBook.Domain;

namespace ShareBook.Service
{
    public class BooksEmailService
    {
        public class BookEmailViewModel
        {
            public Book Book { get; set; }
            public User Administrator { get; set; }
        }

        private readonly IEmailService _emailService;
        private readonly IUserService _userService;
        public BooksEmailService(IEmailService emailService, IUserService userService)
        {
            _emailService = emailService;
            _userService = userService;
        }

        public async Task SendEmailNewBookInserted(Book book)
        {
            if (book.User == null)
                book.User = _userService.Get(book.UserId);

            var administrators = _userService.GetAllAdministrators();

            foreach (var admin in administrators)
                await SendEmailNewBookInsertedToAdministrator(book, admin);
        }

        private async Task SendEmailNewBookInsertedToAdministrator(Book book, User administrator)
        {
            var vm = new
            {
                Book = book,
                Administrator = administrator
            };

            var html = await EmailTemplate.GenerateHtmlFromTemplate("NewBookInsertedTemplate", vm);
            _emailService.Send(administrator.Email, administrator.Name, html, "Novo livro incluído - Sharebook");
        }
    }
}
