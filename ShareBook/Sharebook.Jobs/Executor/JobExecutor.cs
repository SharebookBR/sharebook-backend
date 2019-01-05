using System;
using System.Collections.Generic;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Repository;
using System.Diagnostics;

namespace Sharebook.Jobs
{
    public class JobExecutor : IJobExecutor
    {
        private readonly List<IJob> _jobs = new List<IJob>();
        private readonly IJobHistoryReposiory _jobHistoryRepo;
        private Stopwatch _stopwatch;

        public JobExecutor(IJobHistoryReposiory jobHistoryRepo,
                           ChooseDateReminder job1,
                           LateDonationNotification job2,
                           RemoveBookFromShowcase job3)
        {
            _jobHistoryRepo = jobHistoryRepo;

            _jobs.Add(job1);
            _jobs.Add(job2);
            _jobs.Add(job3);

        }

        public JobExecutorResult Execute()
        {
            _stopwatch = Stopwatch.StartNew();

            var messages = new List<string>();
            var success = true;

            var r = new Result() { 

             };

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
            }

            // Executor também loga seu histórico. Precisamos de rastreabilidade.
            _stopwatch.Stop();
            var details = String.Join("\n", messages.ToArray());
            LogExecutorAddHistory(details);

            return new JobExecutorResult()
            {
                Success = success,
                Messages = messages
            };
        }

        private void LogExecutorAddHistory(string details)
        {
            var history = new JobHistory()
            {
                JobName = "JobExecutor",
                IsSuccess = true,
                Details = details,
                TimeSpentSeconds = ((double)_stopwatch.ElapsedMilliseconds / (double)1000),
            };

            _jobHistoryRepo.Insert(history);
        }
    }
}
