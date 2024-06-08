using Rollbar;
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
    private Stopwatch _stopwatch;

    public JobExecutor(IJobHistoryRepository jobHistoryRepo,
                        CancelAbandonedDonations job0,
                        ChooseDateReminder job1,
                        LateDonationNotification job2,
                        RemoveBookFromShowcase job3,
                        MeetupSearch job4,
                        NewBookGetInterestedUsers job5,
                        MailSupressListUpdate job6,
                        MailSender job7) // MailSender precisa ser o último!
    {
        _jobHistoryRepo = jobHistoryRepo;

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
            job7
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

                    // executa apenas um job por ciclo.
                    break;
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
            SendErrorToRollbar(ex);
        }

        // Executor também loga seu histórico. Precisamos de rastreabilidade.
        _stopwatch.Stop();
        var details = String.Join("\n", messages.ToArray());
        LogExecutorAddHistory(success, details);

        return new JobExecutorResult()
        {
            Success = success,
            Messages = messages
        };
    }

    private void LogExecutorAddHistory(bool success, string details)
    {
        var history = new JobHistory()
        {
            JobName = "JobExecutor",
            IsSuccess = success,
            Details = details,
            TimeSpentSeconds = ((double)_stopwatch.ElapsedMilliseconds / (double)1000),
        };

        _jobHistoryRepo.Insert(history);
    }

    // TODO: criar um service pro rollbar e reaproveitar aqui
    // e no ExceptionHandlerMiddleware.
    private void SendErrorToRollbar(Exception ex)
    {
        object error = new
        {
            Message = ex.Message,
            StackTrace = ex.StackTrace,
            Source = ex.Source
        };

        RollbarLocator.RollbarInstance.Error(error);
    }
}

