using ShareBook.Domain;
using ShareBook.Repository.Repository;
using System;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public class BookUserEmailService : IBookUsersEmailService
    {
        private const string BookRequestedTemplate = "BookRequestedTemplate";
        private const string BookNoticeDonorTemplate = "BookNoticeDonorTemplate";
        private const string BookDonatedTemplate = "BookDonatedTemplate";
        private const string BookDonatedTitle = "Parabéns você foi selecionado!";
        private const string BookRequestedTitle = "Um livro foi solicitado - Sharebook";
        private const string BookNoticeDonorTitle = "Seu livro foi solicitado - Sharebook";

  


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
            if (bookDonated.User == null)
                bookDonated.User = _userService.Find(bookUser.Book.UserId);
            var vm = new
            {
                Book = bookDonated,
                bookUser.User
            };
            var html = await _emailTemplate.GenerateHtmlFromTemplateAsync(BookDonatedTemplate, vm);
            await _emailService.Send(bookUser.User.Email, bookUser.User.Name, html, BookDonatedTitle, true);
        }

        public async Task SendEmailBookRequested(BookUser bookUser)
        {
            var includeList = new IncludeList<Book>(x => x.User);
            var bookRequested = _bookService.Find(includeList, bookUser.BookId);

            var requestingUser = _userService.Find(bookUser.UserId);

            var vm = new
            {
                Request = bookUser,
                Book = bookRequested,
                RequestingUser = requestingUser,
            };
            var html = await _emailTemplate.GenerateHtmlFromTemplateAsync(BookRequestedTemplate, vm);
            await _emailService.SendToAdmins(html, BookRequestedTitle);
        }

        public async Task SendEmailBookDonor(BookUser bookUser)
        {
            var includeList = new IncludeList<Book>(x => x.User);
            var bookRequested = _bookService.Find(includeList, bookUser.BookId);
            var vm = new
            {

                Request = bookUser,
                Book = bookRequested,
                Donor = new
                {
                    Name = bookRequested.User.Name,
                    ChooseDate = string.Format("{0:dd/MM/yyyy}", bookRequested.ChooseDate.Value)
                },
                RequestingUser = new {NickName = $"Interessado {bookRequested.TotalInterested()}" },
            };

            var html = await _emailTemplate.GenerateHtmlFromTemplateAsync(BookNoticeDonorTemplate, vm);

            await _emailService.Send(bookRequested.User.Email, bookRequested.User.Name, html, BookNoticeDonorTitle);

        }
    }
}
