using MailKit;
using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Repository;
using ShareBook.Service;
using ShareBook.Service.AwsSqs;
using System;
using System.Collections.Generic;

namespace Sharebook.Jobs;

public class MailSupressListUpdate : GenericJob, IJob
{
    private readonly IEmailService _emailService;

    public MailSupressListUpdate(
        IJobHistoryRepository jobHistoryRepo,
        IEmailService emailService) : base(jobHistoryRepo)
    {

        JobName = "MailSupressListUpdate";
        Description = @"Atualiza a lista de emails suprimidos. Essa lista serve para manter boa reputação do nosso 
                        mailling. Além de ser um requisito da AWS.";
        Interval = Interval.Hourly;
        Active = true;
        BestTimeToExecute = null;

        _emailService = emailService;
    }

    public override JobHistory Work()
    {
        var log  = _emailService.ProcessBounceMessages().Result;

        return new JobHistory()
        {
            JobName = JobName,
            IsSuccess = true,
            Details = String.Join("\n", log)
        };
    }
}


