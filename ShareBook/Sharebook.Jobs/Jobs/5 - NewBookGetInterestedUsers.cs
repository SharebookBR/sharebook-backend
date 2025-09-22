using Microsoft.Extensions.Configuration;
using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Domain.Exceptions;
using ShareBook.Repository;
using ShareBook.Service;
using ShareBook.Service.AwsSqs;
using ShareBook.Service.AwsSqs.Dto;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sharebook.Jobs;

public class NewBookGetInterestedUsers : GenericJob, IJob
{
    private readonly NewBookQueue _newBookQueue;
    private readonly MailSenderLowPriorityQueue _mailSenderLowPriorityQueue;
    private readonly IBookService _bookService;
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;
    private readonly IEmailTemplate _emailTemplate;

    public NewBookGetInterestedUsers(
        IJobHistoryRepository jobHistoryRepo,
        NewBookQueue newBookQueue,
        MailSenderLowPriorityQueue mailSenderLowPriorityQueue,
        IUserService userService,
        IConfiguration configuration,
        IEmailTemplate emailTemplate,
        IBookService bookService) : base(jobHistoryRepo)
    {

        JobName = "NewBookGetInterestedUsers";
        Description = @"Temos um fluxo em que notificamos novos livros para possíveis interessados. Dentro desse 
                        fluxo, essa peça tem a responsailidade de ler uma fila de LIVROS DOADOS, buscar possíveis 
                        interessados e alimentar a fila do MAIL SENDER (baixa prioridade).";
        Interval = Interval.Hourly;
        Active = true;
        BestTimeToExecute = null;

        _newBookQueue = newBookQueue;
        _mailSenderLowPriorityQueue = mailSenderLowPriorityQueue;
        _userService = userService;
        _configuration = configuration;
        _emailTemplate = emailTemplate;
        _bookService = bookService;
    }

    public override async Task<JobHistory> WorkAsync()
    {
        var awsSqsEnabled = bool.Parse(_configuration["AwsSqsSettings:IsActive"]);
        if(!awsSqsEnabled) throw new AwsSqsDisabledException("Serviço aws sqs está desabilitado no appsettings.");
        
        int totalDestinations = 0;
        int sendEmailMaxDestinationsPerQueueMessage = GetEmailMaxDestinationsPerQueueMessage();
        
        // 1 - lê a fila de origem
        var newBookMessage = await _newBookQueue.GetMessageAsync();

        // fila vazia, não faz nada
        if (newBookMessage == null)
        {
            return new JobHistory() 
            {
                JobName = JobName,
                IsSuccess = true,
                Details = "A fila de origem estava vazia. Não fiz nada."
            };
        }

        // Obtem usuários interessados
        var newBook = newBookMessage.Body;
        var interestedUsers = await _userService.GetBySolicitedBookCategoryAsync(newBook.CategoryId);
        totalDestinations = interestedUsers.Count;
        var template = await GetEmailTemplateAsync(newBook.BookId);

        // Alimenta a fila de destino - baixa prioridade do Mail Sender
        int maxMessages = interestedUsers.Count % sendEmailMaxDestinationsPerQueueMessage == 0 ? interestedUsers.Count / sendEmailMaxDestinationsPerQueueMessage : interestedUsers.Count / sendEmailMaxDestinationsPerQueueMessage + 1;

        for(int i = 1; i <= maxMessages; i++)
        {
            var destinations = interestedUsers.OrderBy(i => i.Id).Skip((i - 1) * sendEmailMaxDestinationsPerQueueMessage).Take(sendEmailMaxDestinationsPerQueueMessage).Select(u => new Destination { Name = u.Name, Email = u.Email });

            var mailSenderbody = new MailSenderbody {
                Subject = $"Chegou o livro '{newBook.BookTitle}'",
                BodyHTML = template,
                Destinations = destinations.ToList()
            };

            await _mailSenderLowPriorityQueue.SendMessageAsync(mailSenderbody);
        }

        // remove a mensagem da fila de origem
        await _newBookQueue.DeleteMessageAsync(newBookMessage.ReceiptHandle);

        // finaliza com sucesso
        return new JobHistory() 
        {
            JobName = JobName,
            IsSuccess = true,
            Details = $"{totalDestinations} usuários encontrados quem podem ter interesse no livro '{newBook?.BookTitle}'."
        };
    }

    private int GetEmailMaxDestinationsPerQueueMessage()
    {
        // O MailSender é invocado 10x a cada hora.
        var mailSenderInvocationsPerHour = 10;

        // Queremos que o MailSender processe 3 mensagens sqs pra fazer seu trabalho
        var totalSqsMessagensPerWork = 3;
        
        var maxEmailsPerHour = int.Parse(_configuration["EmailSettings:MaxEmailsPerHour"]);
        return (maxEmailsPerHour / mailSenderInvocationsPerHour) / totalSqsMessagensPerWork;
    }

    private async Task<string> GetEmailTemplateAsync(Guid bookId){
        var book = await _bookService.FindAsync(bookId);
        
        var vm = new
        {
            BookTitle = book.Title,
            BookSlug = book.Slug,
            BookImageSlug = book.ImageSlug,
            SharebookBaseUrl = _configuration["ServerSettings:FrontendUrl"],
            Name = "{Name}"// o MailSender vai trocar pelo nome do usuário.
        };

        return await _emailTemplate.GenerateHtmlFromTemplateAsync("NewBookNotifyTemplate", vm);
    }
}

