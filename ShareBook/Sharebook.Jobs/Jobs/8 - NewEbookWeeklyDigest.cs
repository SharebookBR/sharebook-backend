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

public class NewEbookWeeklyDigest : GenericJob, IJob
{
    private readonly MailSenderLowPriorityQueue _mailSenderLowPriorityQueue;
    private readonly IBookRepository _bookRepository;
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;
    private readonly IEmailTemplate _emailTemplate;

    public NewEbookWeeklyDigest(
        IJobHistoryRepository jobHistoryRepo,
        MailSenderLowPriorityQueue mailSenderLowPriorityQueue,
        IBookRepository bookRepository,
        IUserService userService,
        IConfiguration configuration,
        IEmailTemplate emailTemplate,
        ILoggerFactory loggerFactory) : base(jobHistoryRepo, loggerFactory)
    {
        JobName = "NewEbookWeeklyDigest";
        Description = @"Digest semanal de ebooks aprovados nos ultimos 7 dias. Envia UM unico email
                        por usuario interessado nas categorias dos ebooks novos.";
        Interval = Interval.Weekly;
        Active = true;
        BestTimeToExecute = new TimeSpan(9, 0, 0);

        _mailSenderLowPriorityQueue = mailSenderLowPriorityQueue;
        _bookRepository = bookRepository;
        _userService = userService;
        _configuration = configuration;
        _emailTemplate = emailTemplate;
    }

    public override async Task<JobHistory> WorkAsync()
    {
        var awsSqsEnabled = bool.Parse(_configuration["AwsSqsSettings:IsActive"]);
        if (!awsSqsEnabled) throw new AwsSqsDisabledException("Servico aws sqs esta desabilitado no appsettings.");

        var since = DateTime.Today.AddDays(-7);
        Logger.LogInformation("{Job} iniciando. Buscando ebooks aprovados desde {Since:yyyy-MM-dd}.", JobName, since);

        var newEbooks = await _bookRepository.Get()
            .Where(b => b.Type == BookType.Eletronic
                     && b.Status == BookStatus.Available
                     && b.ApprovedAt >= since)
            .ToListAsync();

        Logger.LogInformation("{Job} encontrou {Count} ebook(s) aprovado(s) nos ultimos 7 dias.", JobName, newEbooks.Count);

        if (!newEbooks.Any())
        {
            return new JobHistory()
            {
                JobName = JobName,
                IsSuccess = true,
                Details = "Nenhum ebook aprovado nos ultimos 7 dias. Nao fiz nada."
            };
        }

        var userEbooks = new Dictionary<Guid, (User User, List<Book> Ebooks)>();

        foreach (var ebook in newEbooks)
        {
            var interestedUsers = await _userService.GetBySolicitedBookCategoryAsync(ebook.CategoryId);

            foreach (var user in interestedUsers)
            {
                if (!userEbooks.ContainsKey(user.Id))
                    userEbooks[user.Id] = (user, new List<Book>());

                userEbooks[user.Id].Ebooks.Add(ebook);
            }
        }

        if (!userEbooks.Any())
        {
            Logger.LogWarning("{Job} nenhum usuario interessado nas categorias dos ebooks novos.", JobName);
            return new JobHistory()
            {
                JobName = JobName,
                IsSuccess = true,
                Details = $"{newEbooks.Count} ebook(s) encontrado(s), mas nenhum usuario interessado nas categorias."
            };
        }

        var frontendUrl = _configuration["ServerSettings:FrontendUrl"];
        var totalRecipients = 0;

        foreach (var (_, entry) in userEbooks)
        {
            var ebookListHtml = BuildEbookListHtml(entry.Ebooks, frontendUrl);

            var vm = new
            {
                Name = "{name}",
                EbookListHtml = ebookListHtml,
                FrontendUrl = frontendUrl
            };

            var bodyHtml = await _emailTemplate.GenerateHtmlFromTemplateAsync("EbooksWeeklyDigestTemplate", vm);

            var mailBody = new MailSenderbody
            {
                Subject = "Novos livros digitais esta semana",
                BodyHTML = bodyHtml,
                Destinations = new List<Destination>
                {
                    new Destination { Name = entry.User.Name, Email = entry.User.Email }
                }
            };

            await _mailSenderLowPriorityQueue.SendMessageAsync(mailBody);
            totalRecipients++;
        }

        var details = $"{newEbooks.Count} ebook(s) novo(s). {totalRecipients} digest(s) enfileirado(s).";
        Logger.LogInformation("{Job} concluido. {Details}", JobName, details);

        return new JobHistory()
        {
            JobName = JobName,
            IsSuccess = true,
            Details = details
        };
    }

    private static string BuildEbookListHtml(List<Book> ebooks, string frontendUrl)
    {
        var sb = new StringBuilder();
        foreach (var ebook in ebooks)
        {
            sb.Append($@"<li style=""margin-bottom:10px;"">
                <a href=""{frontendUrl}/livros/{ebook.Slug}"" style=""color:#009FC7;font-weight:bold;text-decoration:none;"">
                    {ebook.Title}
                </a>
                <span style=""color:#757575;""> — {ebook.Author}</span>
            </li>");
        }
        return sb.ToString();
    }
}
