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
    public class MailSender : GenericJob, IJob
    {
        private readonly IEmailService _emailService;
        private readonly MailSenderLowPriorityQueue _sqs;

        public MailSender(
            IJobHistoryRepository jobHistoryRepo,
            IEmailService emailService,
            MailSenderLowPriorityQueue sqs) : base(jobHistoryRepo)
        {

            JobName = "MailSender";
            Description = "Assim que um novo livro é aprovado, notifica, por e-mail, os usuários que já solicitaram algum livro da mesma categoria do novo. " +
                          "Para isso é utilizada a leitura de uma fila da Amazon SQS.";
            Interval = Interval.Each5Minutes;
            Active = false;
            BestTimeToExecute = null;

            _emailService = emailService;
            _sqs = sqs;
        }

        public override JobHistory Work()
        {
            int qtDestinations = 0;

            var envelope = _sqs.GetMessage().Result;
            var message = envelope.Body;

            if (message != null)
            {
                foreach (var destination in message.Destinations)
                {
                    _emailService.Send(destination.Email, destination.Name, message.BodyHTML.Replace("{name}", destination.Name), message.Subject).Wait();

                    // freio lógico
                    Thread.Sleep(1000);
                }

                var receiptHandle = message.ReceiptHandle;
                _sqs.DeleteMessage(receiptHandle).Wait();

                qtDestinations = message.Destinations.Count();
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
