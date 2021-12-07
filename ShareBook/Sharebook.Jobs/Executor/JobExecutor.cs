using Rollbar;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Repository;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Sharebook.Jobs
{
    // inspirado no design pattern chain of responsability
    // https://pt.wikipedia.org/wiki/Chain_of_Responsibility

    public class JobExecutor : IJobExecutor
    {
        private readonly IList<IJob> _jobs;
        private readonly IJobHistoryRepository _jobHistoryRepo;
        private Stopwatch _stopwatch;

        public JobExecutor(IJobHistoryRepository jobHistoryRepo,
                           ChooseDateReminder job1,
                           LateDonationNotification job2,
                           RemoveBookFromShowcase job3,
                           NewBookNotify job4)
        {
            _jobHistoryRepo = jobHistoryRepo;

            _jobs = new List<IJob>
            {
                job1,
                job2,
                job3,
                job4
            };

        }

        public JobExecutorResult Execute()
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

                        if (job.Execute())
                        {
                            messages.Add(string.Format("Job {0}: job executado com sucesso.", job.JobName));
                        }
                        else
                        {
                            success = false;
                            messages.Add(string.Format("Job {0}: ocorreu um erro ao executar o job. Verifique os logs.", job.JobName));
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
            catch(Exception ex)
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
}
