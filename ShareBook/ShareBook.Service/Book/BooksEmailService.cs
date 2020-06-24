using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
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
        private const string NewBookNotifyTemplate = "NewBookNotifyTemplate";
        private const string BookReceivedTemplate = "BookReceivedTemplate";

        private readonly IEmailService _emailService;
        private readonly IUserService _userService;
        private readonly IEmailTemplate _emailTemplate;
        private readonly IAWSSQSService _AWSSQSService;
        private readonly ServerSettings _serverSettings;
        private readonly IConfiguration _configuration;

        public BooksEmailService(
            IEmailService emailService,
            IUserService userService,
            IEmailTemplate emailTemplate,
            IAWSSQSService AWSSQSService,
            IOptions<ServerSettings> serverSettings,
            IConfiguration configuration)
        {
            _emailService = emailService;
            _userService = userService;
            _emailTemplate = emailTemplate;
            _AWSSQSService = AWSSQSService;
            _serverSettings = serverSettings.Value;
            _configuration = configuration;
        }

        public void SendEmailBookApproved(Book book)
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
                var html = _emailTemplate.GenerateHtmlFromTemplateAsync(BookApprovedTemplate, vm).Result;
                _emailService.Send(book.User.Email, book.User.Name, html, BookApprovedTitle, true);
            }
        }

        public void SendEmailBookReceived(Book book)
        {
            if (book.User == null)
                book.User = _userService.Find(book.UserId);

            if (book.User.AllowSendingEmail)
            {
                var vm = new
                {
                    Book = book,
                    book.User,
                    WinnerName = book.WinnerName(),
                };

                var htmt = _emailTemplate.GenerateHtmlFromTemplateAsync(BookReceivedTemplate, vm).Result;
                _emailService.Send(book.User.Email, book.User.Name, htmt, BookReceivedTemplate, true);
            }
        }

        public async Task SendEmailBookToInterestedUsers(Book book)
        {
            int MAX_DESTINATIONS = int.Parse(_configuration["AWSSQSSettings:MaxDestinationsPerMessage"]);

            var vm = new
                {
                    Book = book,
                    ServerSettings = _serverSettings,
                    name = "{name}"// recebe {name} pois e o padrao que o servico Consumer reconhece para substituicao para o nome do usuario para o qual ira enviar o email
                };

            var html = await _emailTemplate.GenerateHtmlFromTemplateAsync(NewBookNotifyTemplate, vm);

            var message = new AWSSQSMessageNewBookNotifyRequest
            {
                Subject = $"Chegou um livro de {book.Category.Name}",
                BodyHTML = html
            };


            var interestedUsers = _userService.GetBySolicitedBookCategory(book.CategoryId);


            int maxMessages = interestedUsers.Count() % MAX_DESTINATIONS == 0 ? interestedUsers.Count() / MAX_DESTINATIONS : interestedUsers.Count() / MAX_DESTINATIONS + 1;

            for(int i = 1; i <= maxMessages; i++)
            {
                var destinations = interestedUsers.Skip((i - 1) * MAX_DESTINATIONS).Take(MAX_DESTINATIONS).Select(u => new DestinationRequest { Name = u.Name, Email = u.Email });
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
