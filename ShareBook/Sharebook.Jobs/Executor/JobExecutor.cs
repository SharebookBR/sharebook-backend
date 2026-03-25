using Microsoft.Extensions.Logging;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Domain.Enums;
using ShareBook.Repository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Sharebook.Jobs;

// inspirado no design pattern chain of responsability
// https://pt.wikipedia.org/wiki/Chain_of_Responsibility

public class JobExecutor : IJobExecutor
{
    private readonly IList<IJob> _jobs;
    private readonly IJobHistoryRepository _jobHistoryRepo;
    private readonly ILogger<JobExecutor> _logger;
    private Stopwatch _stopwatch;

    public JobExecutor(IJobHistoryRepository jobHistoryRepo,
                        ILogger<JobExecutor> logger,
                        CancelAbandonedDonations job0,
                        ChooseDateReminder job1,
                        LateDonationNotification job2,
                        RemoveBookFromShowcase job3,
                        MeetupSearch job4,
                        NewBookGetInterestedUsers job5,
                        MailSupressListUpdate job6,
                        MailSender job7, // MailSender precisa ser o último!
                        NewEbookWeeklyDigest job8)
    {
        _jobHistoryRepo = jobHistoryRepo;
        _logger = logger;

        // TODO: obter a lista automaticamente via reflection
        _jobs = new List<IJob>
        {
            job0,
            job1,
            job2,
            job3,
            job4,
            job5,
            job6,
            job7,
            job8
        };

    }

    public async Task<JobExecutorResult> ExecuteAsync()
    {
        _stopwatch = Stopwatch.StartNew();

        var messages = new List<string>();
        var success = true;

        try
        {
            foreach (IJob job in _jobs)
            {
                if (!job.Active)
                {
                    messages.Add(string.Format("Job {0}: job não foi executado porque está INATIVO.", job.JobName));
                    continue;
                }

                if (job.HasWork())
                {

                    var result = await job.ExecuteAsync();

                    switch(result)
                    {
                        case JobResult.Success: 
                            messages.Add(string.Format("Job {0}: job executado com sucesso.", job.JobName));
                            break;

                        case JobResult.Error: 
                            success = false;
                            messages.Add(string.Format("Job {0}: ocorreu um erro ao executar o job. Verifique os logs.", job.JobName));
                        break;

                        case JobResult.AwsSqsDisabled: 
                            success = true;
                            messages.Add(string.Format("Job {0}: não foi executado porque o serviço AWS SQS ESTÁ DESATIVADO.", job.JobName));
                            continue;

                        case JobResult.MeetupDisabled: 
                            success = true;
                            messages.Add(string.Format("Job {0}: não foi executado porque o serviço MEETUP ESTÁ DESATIVADO.", job.JobName));
                            continue;
                    }
                }
                else
                {
                    messages.Add(string.Format("Job {0}: não tinha nenhum trabalho a ser feito.", job.JobName));
                }
            }
        }
        catch (Exception ex)
        {
            success = false;
            messages.Add(string.Format("Executor: ocorreu um erro fatal. {0}", ex.Message));
            _logger.LogError(ex, "JobExecutor falhou com erro fatal");
        }

        // Executor também loga seu histórico. Precisamos de rastreabilidade.
        _stopwatch.Stop();
        var details = String.Join("\n", messages.ToArray());
        await LogExecutorAddHistoryAsync(success, details);

        return new JobExecutorResult()
        {
            Success = success,
            Messages = messages
        };
    }

    private async Task LogExecutorAddHistoryAsync(bool success, string details)
    {
        var history = new JobHistory()
        {
            JobName = "JobExecutor",
            IsSuccess = success,
            Details = details,
            TimeSpentSeconds = ((double)_stopwatch.ElapsedMilliseconds / (double)1000),
        };

        await _jobHistoryRepo.InsertAsync(history);
    }

}
