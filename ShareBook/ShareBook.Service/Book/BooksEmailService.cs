using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Math.EC.Rfc7748;
using ShareBook.Domain;
using ShareBook.Service.AWSSQS;
using ShareBook.Service.AWSSQS.Dto;
using ShareBook.Service.Server;
using System;
using System.Linq;
using System.Security.Cryptography;
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
        private readonly IAWSSQSService _AWSSQSService;
        private readonly ServerSettings _serverSettings;
        public BooksEmailService(IEmailService emailService, IUserService userService, IEmailTemplate emailTemplate, IAWSSQSService AWSSQSService, IOptions<ServerSettings> serverSettings)
        {
            _emailService = emailService;
            _userService = userService;
            _emailTemplate = emailTemplate;
            _AWSSQSService = AWSSQSService;
            _serverSettings = serverSettings.Value;
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

        public async Task SendEmailBookToInterestedUsers(Book book)
        {
            const int MAX_DESTINATIONS = 50;


            var message = new AWSSQSMessageNewBookNotify
            {
                Subject = $"Chegou um livro de {book.Category.Name}",
                BodyHTML = "Olá {name}, chegou um livro do seu interesse. <br>" + $"<img src=\"{_serverSettings.DefaultUrl}/Images/Books/{book.ImageSlug}\"> <br><br><a href=\"{_serverSettings.DefaultUrl}/livros/{book.Slug}\">Quero esse livro!</a>",
            };


            var interestedUsers = _userService.GetBySolicitedBookCategory(book.CategoryId);


            int maxMessages = interestedUsers.Count() % MAX_DESTINATIONS == 0 ? interestedUsers.Count() / MAX_DESTINATIONS : interestedUsers.Count() / MAX_DESTINATIONS + 1;

            for(int i = 1; i <= maxMessages; i++)
            {
                var destinations = interestedUsers.Skip((i - 1) * MAX_DESTINATIONS).Take(MAX_DESTINATIONS).Select(u => new Destination { Name = u.Name, Email = u.Email });
                message.Destinations = destinations.ToList();

                await _AWSSQSService.SendNewBookNotifyToAWSSQSAsync(message);
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
    }
}
