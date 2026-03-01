using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Domain.Exceptions;
using ShareBook.Repository;
using ShareBook.Service;
using ShareBook.Service.AwsSqs;
using ShareBook.Service.AwsSqs.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharebook.Jobs;

public class NewBookGetInterestedUsers : GenericJob, IJob
{
    private readonly MailSenderLowPriorityQueue _mailSenderLowPriorityQueue;
    private readonly IBookRepository _bookRepository;
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;
    private readonly IEmailTemplate _emailTemplate;

    public NewBookGetInterestedUsers(
        IJobHistoryRepository jobHistoryRepo,
        MailSenderLowPriorityQueue mailSenderLowPriorityQueue,
        IUserService userService,
        IConfiguration configuration,
        IEmailTemplate emailTemplate,
        ILoggerFactory loggerFactory,
        IBookRepository bookRepository) : base(jobHistoryRepo, loggerFactory)
    {
        JobName = "NewBookGetInterestedUsers";
        Description = @"Digest diário de livros físicos aprovados nas últimas 24h. Agrupa por usuário
                        interessado na categoria e envia UM único email com todos os livros relevantes.";
        Interval = Interval.Dayly;
        Active = true;
        BestTimeToExecute = new TimeSpan(8, 0, 0);

        _mailSenderLowPriorityQueue = mailSenderLowPriorityQueue;
        _userService = userService;
        _configuration = configuration;
        _emailTemplate = emailTemplate;
        _bookRepository = bookRepository;
    }

    public override async Task<JobHistory> WorkAsync()
    {
        var awsSqsEnabled = bool.Parse(_configuration["AwsSqsSettings:IsActive"]);
        if (!awsSqsEnabled) throw new AwsSqsDisabledException("Serviço aws sqs está desabilitado no appsettings.");

        var since = DateTime.Today.AddDays(-1);

        var newBooks = await _bookRepository.Get()
            .Where(b => b.Type == BookType.Printed
                     && b.Status == BookStatus.Available
                     && b.ApprovedAt >= since)
            .ToListAsync();

        if (!newBooks.Any())
        {
            return new JobHistory()
            {
                JobName = JobName,
                IsSuccess = true,
                Details = "Nenhum livro físico aprovado nas últimas 24h. Não fiz nada."
            };
        }

        // agrupa por usuário: um usuário pode ter interesse em múltiplas categorias
        var userBooks = new Dictionary<Guid, (User User, List<Book> Books)>();

        foreach (var book in newBooks)
        {
            var interestedUsers = await _userService.GetBySolicitedBookCategoryAsync(book.CategoryId);

            foreach (var user in interestedUsers)
            {
                if (!userBooks.ContainsKey(user.Id))
                    userBooks[user.Id] = (user, new List<Book>());

                userBooks[user.Id].Books.Add(book);
            }
        }

        if (!userBooks.Any())
        {
            return new JobHistory()
            {
                JobName = JobName,
                IsSuccess = true,
                Details = $"{newBooks.Count} livro(s) encontrado(s), mas nenhum usuário interessado nas categorias."
            };
        }

        var frontendUrl = _configuration["ServerSettings:FrontendUrl"];
        var totalEmailsQueued = 0;

        foreach (var (userId, entry) in userBooks)
        {
            var bookListHtml = BuildBookListHtml(entry.Books, frontendUrl);

            var vm = new
            {
                Name = "{name}", // o MailSender substitui pelo nome do destinatário
                BookListHtml = bookListHtml,
                FrontendUrl = frontendUrl
            };

            var bodyHtml = await _emailTemplate.GenerateHtmlFromTemplateAsync("PrintedBooksDigestTemplate", vm);

            var mailBody = new MailSenderbody
            {
                Subject = "Novos livros físicos para você hoje",
                BodyHTML = bodyHtml,
                Destinations = new List<Destination>
                {
                    new Destination { Name = entry.User.Name, Email = entry.User.Email }
                }
            };

            await _mailSenderLowPriorityQueue.SendMessageAsync(mailBody);
            totalEmailsQueued++;
        }

        return new JobHistory()
        {
            JobName = JobName,
            IsSuccess = true,
            Details = $"{newBooks.Count} livro(s) físico(s) novo(s). {totalEmailsQueued} digest(s) enfileirado(s)."
        };
    }

    private static string BuildBookListHtml(List<Book> books, string frontendUrl)
    {
        var sb = new StringBuilder();
        foreach (var book in books)
        {
            sb.Append($@"<li style=""margin-bottom:10px;"">
                <a href=""{frontendUrl}/livros/{book.Slug}"" style=""color:#009FC7;font-weight:bold;text-decoration:none;"">
                    {book.Title}
                </a>
                <span style=""color:#757575;""> — {book.Author}</span>
            </li>");
        }
        return sb.ToString();
    }
}

