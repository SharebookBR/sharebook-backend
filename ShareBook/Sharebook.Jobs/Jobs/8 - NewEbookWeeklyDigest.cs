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
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly IEmailTemplate _emailTemplate;

    public NewEbookWeeklyDigest(
        IJobHistoryRepository jobHistoryRepo,
        MailSenderLowPriorityQueue mailSenderLowPriorityQueue,
        IBookRepository bookRepository,
        IUserRepository userRepository,
        IConfiguration configuration,
        IEmailTemplate emailTemplate,
        ILoggerFactory loggerFactory) : base(jobHistoryRepo, loggerFactory)
    {
        JobName = "NewEbookWeeklyDigest";
        Description = @"Digest semanal de ebooks aprovados nos últimos 7 dias. Envia UM único email
                        para todos os usuários com AllowSendingEmail = true listando os ebooks disponíveis.";
        Interval = Interval.Weekly;
        Active = false;
        BestTimeToExecute = new TimeSpan(9, 0, 0);

        _mailSenderLowPriorityQueue = mailSenderLowPriorityQueue;
        _bookRepository = bookRepository;
        _userRepository = userRepository;
        _configuration = configuration;
        _emailTemplate = emailTemplate;
    }

    public override async Task<JobHistory> WorkAsync()
    {
        var awsSqsEnabled = bool.Parse(_configuration["AwsSqsSettings:IsActive"]);
        if (!awsSqsEnabled) throw new AwsSqsDisabledException("Serviço aws sqs está desabilitado no appsettings.");

        var since = DateTime.Today.AddDays(-7);
        Logger.LogInformation("{Job} iniciando. Buscando ebooks aprovados desde {Since:yyyy-MM-dd}.", JobName, since);

        var newEbooks = await _bookRepository.Get()
            .Where(b => b.Type == BookType.Eletronic
                     && b.Status == BookStatus.Available
                     && b.ApprovedAt >= since)
            .ToListAsync();

        Logger.LogInformation("{Job} encontrou {Count} ebook(s) aprovado(s) nos últimos 7 dias.", JobName, newEbooks.Count);

        if (!newEbooks.Any())
        {
            return new JobHistory()
            {
                JobName = JobName,
                IsSuccess = true,
                Details = "Nenhum ebook aprovado nos últimos 7 dias. Não fiz nada."
            };
        }

        var frontendUrl = _configuration["ServerSettings:FrontendUrl"];
        var ebookListHtml = BuildEbookListHtml(newEbooks, frontendUrl);

        var vm = new
        {
            Name = "{name}", // o MailSender substitui pelo nome do destinatário
            EbookListHtml = ebookListHtml,
            FrontendUrl = frontendUrl
        };

        var bodyHtml = await _emailTemplate.GenerateHtmlFromTemplateAsync("EbooksWeeklyDigestTemplate", vm);

        // Pagina direto no banco — nunca carrega todos os usuários em memória de uma vez
        const int batchSize = 50;
        var page = 0;
        var totalRecipients = 0;
        var totalBatches = 0;

        while (true)
        {
            var batch = await _userRepository.Get()
                .Where(u => u.AllowSendingEmail)
                .OrderBy(u => u.Id)
                .Select(u => new Destination { Name = u.Name, Email = u.Email })
                .Skip(page * batchSize)
                .Take(batchSize)
                .ToListAsync();

            if (!batch.Any())
            {
                Logger.LogInformation("{Job} sem mais destinatários. Encerrando paginação na página {Page}.", JobName, page);
                break;
            }

            Logger.LogInformation("{Job} página {Page}: {BatchCount} destinatário(s). Enfileirando mensagem SQS.", JobName, page, batch.Count);

            var mailBody = new MailSenderbody
            {
                Subject = "Novos livros digitais esta semana",
                BodyHTML = bodyHtml,
                Destinations = batch
            };

            await _mailSenderLowPriorityQueue.SendMessageAsync(mailBody);

            totalRecipients += batch.Count;
            totalBatches++;
            page++;
        }

        if (totalRecipients == 0)
        {
            Logger.LogWarning("{Job} nenhum usuário com AllowSendingEmail ativo encontrado.", JobName);
            return new JobHistory()
            {
                JobName = JobName,
                IsSuccess = true,
                Details = $"{newEbooks.Count} ebook(s) encontrado(s), mas nenhum usuário com AllowSendingEmail ativo."
            };
        }

        var details = $"{newEbooks.Count} ebook(s) novo(s). {totalRecipients} destinatário(s) em {totalBatches} lote(s) enfileirado(s).";
        Logger.LogInformation("{Job} concluído. {Details}", JobName, details);

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
