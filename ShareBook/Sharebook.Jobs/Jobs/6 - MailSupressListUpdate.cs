using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Repository;
using ShareBook.Service;
using System;
using System.Threading.Tasks;

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
        Interval = Interval.Dayly;

        // TODO: usar webhook nos novos serviços de email como resend e demais.
        Active = false;
        BestTimeToExecute = new TimeSpan(2, 0, 0);

        _emailService = emailService;
    }

    public override async Task<JobHistory> WorkAsync()
    {
        var log = await _emailService.ProcessBounceMessagesAsync();

        return new JobHistory()
        {
            JobName = JobName,
            IsSuccess = true,
            Details = String.Join("\n", log)
        };
    }
}


