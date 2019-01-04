using System;
using ShareBook.Domain;
using ShareBook.Repository;
using ShareBook.Repository.Repository;
using ShareBook.Domain.Enums;
using System.Collections.Generic;

namespace Sharebook.Jobs
{
    public class RemoveBookFromShowcase : Job, IJob
    {
        private readonly IJobReposiory _jobRepo;
        private IJobHistoryReposiory _jobHistoryRepo;

        public RemoveBookFromShowcase(IJobReposiory jobRepo, IJobHistoryReposiory jobHistoryRepo)
        {
            _jobRepo = jobRepo;

            Name = "RemoveBookFromShowcase";

            // TODO: tratar job não encontrado no banco.

            // 1 - obter dados do job e history no banco

            var includes = new IncludeList<Job>(j => j.JobHistory);
            var jobModel = _jobRepo.Find(includes, j => j.Name == Name);


            // está na hora de rodar o job? Passou o intervalo?
            var DateLimit = GetDateLimitByInterval(jobModel.Interval);

            var jobHistory = jobModel.JobHistory;


            //_jobRepo.Get(). e => e.Email.Equals(user.Email, StringComparison.InvariantCultureIgnoreCase)
            //.Where(x => x.BookId == bookId && x.Status == DonationStatus.WaitingAction)
            //.Select(x => x.User.Cleanup()).ToList();





            var x = 1;
        }

        public bool HasWork()
        {
            return false;
        }

        public bool Execute()
        {
            return true;
        }

        public DateTime GetDateLimitByInterval(Interval i)
        {
            var result = DateTime.Now;

            switch (i)
            {
                case Interval.Dayly: {
                        result = result.AddDays(-1);
                    break;
                }
                case Interval.Hourly:
                {
                        result = result.AddHours(-1);
                    break;
                }
                case Interval.Weekly:
                {
                        result = result.AddDays(-7);
                    break;
                }
            }

            return result;
        }
    }
}
