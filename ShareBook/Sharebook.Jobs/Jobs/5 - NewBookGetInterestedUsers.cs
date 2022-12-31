using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Repository;
using ShareBook.Service;
using ShareBook.Service.AwsSqs;
using System;
using System.Linq;
using System.Threading;

namespace Sharebook.Jobs
{
    public class NewBookGetInterestedUsers : GenericJob, IJob
    {
        private readonly IEmailService _emailService;
        private readonly NewBookQueue _sqs;

        public NewBookGetInterestedUsers(
            IJobHistoryRepository jobHistoryRepo,
            IEmailService emailService,
            NewBookQueue sqs) : base(jobHistoryRepo)
        {

            JobName = "NewBookGetInterestedUsers";
            Description = @"Temos um fluxo em que notificamos novos livros para possíveis interessados. Dentro desse 
                            fluxo, essa peça tem a responsailidade de ler uma fila de LIVROS DOADOS, buscar possíveis 
                            interessados e alimentar a fila do MAIL SENDER (baixa prioridade).";
            Interval = Interval.Hourly;
            Active = true;
            BestTimeToExecute = null;

            _emailService = emailService;
            _sqs = sqs;
        }

        public override JobHistory Work()
        {
            int qtDestinations = 0;

            var message = _sqs.GetMessage().Result;

            if (message != null)
            {
                // TODO: implementar

                // foreach (var destination in message.Destinations)
                // {
                //     _emailService.Send(destination.Email, destination.Name, message.BodyHTML.Replace("{name}", destination.Name), message.Subject).Wait();

                //     // freio lógico
                //     Thread.Sleep(100);
                // }

                // var receiptHandle = message.ReceiptHandle;
                // _sqs.DeleteNewBookNotifyFromAWSSQSAsync(receiptHandle).Wait();

                // qtDestinations = message.Destinations.Count();
            }

            return new JobHistory()
            {
                JobName = JobName,
                IsSuccess = true,
                Details = String.Join("\n", $"{qtDestinations} e-mails enviados.")
            };
        }
    }

}
