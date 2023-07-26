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

    public override JobHistory Work()
    {
        var awsSqsEnabled = bool.Parse(_configuration["AwsSqsSettings:IsActive"]);
        if(!awsSqsEnabled) throw new AwsSqsDisabledException("Serviço aws sqs está desabilitado no appsettings.");
        
        int totalDestinations = 0;
        int sendEmailMaxDestinationsPerQueueMessage = GetEmailMaxDestinationsPerQueueMessage();
        
        // 1 - lê a fila de origem
        var newBookMessage = _newBookQueue.GetMessage()?.Result;

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
        var interestedUsers = _userService.GetBySolicitedBookCategory(newBook.CategoryId);
        totalDestinations = interestedUsers.Count;
        var template = GetEmailTemplate(newBook.BookId);

        // Alimenta a fila de destino - baixa prioridade do Mail Sender
        int maxMessages = interestedUsers.Count % sendEmailMaxDestinationsPerQueueMessage == 0 ? interestedUsers.Count / sendEmailMaxDestinationsPerQueueMessage : interestedUsers.Count / sendEmailMaxDestinationsPerQueueMessage + 1;

        for(int i = 1; i <= maxMessages; i++)
        {
            var destinations = interestedUsers.Skip((i - 1) * sendEmailMaxDestinationsPerQueueMessage).Take(sendEmailMaxDestinationsPerQueueMessage).Select(u => new Destination { Name = u.Name, Email = u.Email });

            var mailSenderbody = new MailSenderbody {
                Subject = $"Chegou o livro '{newBook.BookTitle}'",
                BodyHTML = template,
                Destinations = destinations.ToList()
            };
            
            _mailSenderLowPriorityQueue.SendMessage(mailSenderbody).Wait();
        }

        // remove a mensagem da fila de origem
        _newBookQueue.DeleteMessage(newBookMessage.ReceiptHandle).Wait();

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

        // 
    }

    private string GetEmailTemplate(Guid bookId){
        var book = _bookService.Find(bookId);
        
        var vm = new
        {
            BookTitle = book.Title,
            BookSlug = book.Slug,
            BookImageSlug = book.ImageSlug,
            SharebookBaseUrl = _configuration["ServerSettings:DefaultUrl"],
            Name = "{Name}"// o MailSender vai trocar pelo nome do usuário.
        };

        return _emailTemplate.GenerateHtmlFromTemplateAsync("NewBookNotifyTemplate", vm).Result;
    }
}

