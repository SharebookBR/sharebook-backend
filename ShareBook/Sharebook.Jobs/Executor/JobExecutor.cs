using System;
using System.Collections.Generic;
using ShareBook.Domain.Common;

namespace Sharebook.Jobs
{
    public class JobExecutor : IJobExecutor
    {
        List<IJob> _jobs = new List<IJob>();

        public JobExecutor(RemoveBookFromShowcase job1,
                           ChooseDateReminder job2,
                           LateDonationNotification job3)
        {
            _jobs.Add(job1);
            _jobs.Add(job2);
            _jobs.Add(job3);
        }

        public JobExecutorResult Execute()
        {
            var messages = new List<string>();
            var success = true;

            var r = new Result() { 

             };

            try
            {
                foreach (IJob job in _jobs)
                {
                    if (job.HasWork())
                    {

                        if (job.Execute())
                        {
                            messages.Add(string.Format("Job {0}: job executado com sucesso.", job.Name));
                        }
                        else
                        {
                            success = false;
                            messages.Add(string.Format("Job {0}: ocorreu um erro ao executar o job. Verifique os logs.", job.Name));
                        }

                        // executa apenas um job por ciclo.
                        break;
                    }
                    else
                    {
                        messages.Add(string.Format("Job {0}: não tinha nenhum trabalho a ser feito.", job.Name));
                    }
                }
            }
            catch(Exception ex)
            {
                success = false;
                messages.Add(string.Format("Executor: ocorreu um erro fatal. {0}", ex.Message));
            }

            return new JobExecutorResult()
            {
                Success = success,
                Messages = messages
            };
        }
    }
}
