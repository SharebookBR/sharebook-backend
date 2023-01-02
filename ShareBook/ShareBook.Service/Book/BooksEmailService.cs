using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Math.EC.Rfc7748;
using ShareBook.Domain;
using ShareBook.Domain.DTOs;
using ShareBook.Service.AwsSqs;
using ShareBook.Service.AwsSqs.Dto;
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
        private readonly ServerSettings _serverSettings;
        private readonly IConfiguration _configuration;
        private readonly MailSenderHighPriorityQueue _mailSenderHighPriorityQueue;

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
            _serverSettings = serverSettings.Value;
            _configuration = configuration;
            _mailSenderHighPriorityQueue = mailSenderHighPriorityQueue;
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
                await _emailService.Send(book.User.Email, book.User.Name, html, BookApprovedTitle, copyAdmins: true);
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

        public async Task SendEmailNewBookInserted(Book book)
        {
            if (book.User == null)
                book.User = _userService.Find(book.UserId);

            var userStats = _userService.GetStats(book.UserId);

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
            _mailSenderHighPriorityQueue.SendToAdmins(html, NewBookInsertedTitle);
        }

        private async Task SendEmailWaitingApprovalToUser(Book book)
        {
            if (book.User.AllowSendingEmail)
            {
                var html = await _emailTemplate.GenerateHtmlFromTemplateAsync(WaitingApprovalTemplate, book);

                await _emailService.Send(book.User.Email, book.User.Name, html, WaitingApprovalTitle, copyAdmins: false);
            }
        }
    }
}
