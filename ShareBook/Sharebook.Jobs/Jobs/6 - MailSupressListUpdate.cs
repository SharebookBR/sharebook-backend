using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Repository;
using ShareBook.Service;
using ShareBook.Service.AwsSqs;
using System;
using System.Linq;
using System.Threading;

namespace Sharebook.Jobs;

public class MailSupressListUpdate : GenericJob, IJob
{
    private readonly IEmailService _emailService;
    private readonly MailSenderQueue _sqs; // TODO: criar uma fila pra esse cara

    public MailSupressListUpdate(
        IJobHistoryRepository jobHistoryRepo,
        IEmailService emailService,
        MailSenderQueue sqs) : base(jobHistoryRepo)
    {

        JobName = "MailSupressListUpdate";
        Description = @"Atualiza a lista de emails suprimidos. Essa lista serve para manter boa reputação do nosso 
                        mailling. Além de ser um requisito da AWS.";
        Interval = Interval.Dayly;
        Active = false;
        BestTimeToExecute = new TimeSpan(1, 0, 0);

        _emailService = emailService;
        _sqs = sqs;
    }

    public override JobHistory Work()
    {
        throw new NotImplementedException();
    }
}


