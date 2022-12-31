using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Repository;
using ShareBook.Service;
using ShareBook.Service.AwsSqs;
using System;
using System.Linq;
using System.Threading;

namespace Sharebook.Jobs;

public class NewBookGetInterestedUsers : GenericJob, IJob
{
    private readonly NewBookQueue _newBookQueue;
    private readonly IUserService _userService;

    public NewBookGetInterestedUsers(
        IJobHistoryRepository jobHistoryRepo,
        IEmailService emailService,
        NewBookQueue newBookQueue,
        IUserService userService) : base(jobHistoryRepo)
    {

        JobName = "NewBookGetInterestedUsers";
        Description = @"Temos um fluxo em que notificamos novos livros para possíveis interessados. Dentro desse 
                        fluxo, essa peça tem a responsailidade de ler uma fila de LIVROS DOADOS, buscar possíveis 
                        interessados e alimentar a fila do MAIL SENDER (baixa prioridade).";
        Interval = Interval.Hourly;
        Active = true;
        BestTimeToExecute = null;

        _newBookQueue = newBookQueue;
        _userService = userService;
    }

    public override JobHistory Work()
    {
        int qtDestinations = 0;

        var newBookMessage = _newBookQueue.GetMessage().Result;

        if (newBookMessage != null)
        {
            // TODO: 
            // 1 - criar a fila de baixa prioridade
            // 2 - obter usuários interessados
            // 3 - alimentar a fila de baixa prioridade


            // 2 - obter usuários interessados
            var interestedUsers = _userService.GetBySolicitedBookCategory(newBookMessage.CategoryId);


            // 3 - alimentar a fila de baixa prioridade
            // int maxMessages = interestedUsers.Count() % MAX_DESTINATIONS == 0 ? interestedUsers.Count() / MAX_DESTINATIONS : interestedUsers.Count() / MAX_DESTINATIONS + 1;

            // for(int i = 1; i <= maxMessages; i++)
            // {
            //     var destinations = interestedUsers.Skip((i - 1) * MAX_DESTINATIONS).Take(MAX_DESTINATIONS).Select(u => new Destination { Name = u.Name, Email = u.Email });
            //     message.Destinations = destinations.ToList();

            //     await _AWSSQSService.SendNewBookNotifyToAWSSQSAsync(message);
            // }

        }

        return new JobHistory()
        {
            JobName = JobName,
            IsSuccess = true,
            Details = String.Join("\n", $"{qtDestinations} e-mails enviados.")
        };
    }
}

