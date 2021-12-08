using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Helper;
using ShareBook.Repository;
using System;
using System.Diagnostics;
using System.Linq;

namespace Sharebook.Jobs
{
    public class GenericJob
    {
        public string JobName { get; set; }
        public string Description { get; set; }
        public Interval Interval { get; set; }
        public bool Active { get; set; }
        public TimeSpan? BestTimeToExecute { get; set; }

        protected readonly IJobHistoryRepository _jobHistoryRepo;

        protected Stopwatch _stopwatch;

        public GenericJob(IJobHistoryRepository jobHistoryRepo)
        {
            _jobHistoryRepo = jobHistoryRepo;
        }

        public bool HasWork()
        {
            if(BestTimeToExecute != null)
            {
                var timeNow = DateTimeHelper.GetTimeNowSaoPaulo();
                if (timeNow < BestTimeToExecute) return false;
            }

            var DateLimit = GetDateLimitByInterval(Interval);

            var hasHistory =
            _jobHistoryRepo.Get()
            .Where(x => x.CreationDate > DateLimit &&
                   x.JobName == JobName &&
                   x.IsSuccess == true)
            .ToList().Any();

            return !hasHistory;
        }

        public DateTime GetDateLimitByInterval(Interval i)
        {
            var result = DateTime.Now;

            switch (i)
            {
                case Interval.Dayly:
                {
                    result = DateTimeHelper.GetTodaySaoPaulo();
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
                case Interval.Each30Minutes:
                {
                    result = result.AddMinutes(-30);
                    break;
                }
                case Interval.Each5Minutes:
                {
                    result = result.AddMinutes(-5);
                    break;
                }
            }

            // ajuste de +1 minuto, levando em consideração o tempo que o job
            // pode precisar para completar em sua última execução.
            result = result.AddMinutes(+1);
            return result;
        }

        public bool Execute()
        {
            BeforeWork();
            var history = Work();
            AfterWork(history);

            return history.IsSuccess;
        }

        // Sempre sobrescrito pelo Job real.
        public virtual JobHistory Work() => new JobHistory();

        protected void BeforeWork()
        {
            _stopwatch = Stopwatch.StartNew();
        }

        protected void AfterWork(JobHistory history)
        {
            _stopwatch.Stop();

            history.TimeSpentSeconds = ((double)_stopwatch.ElapsedMilliseconds / (double)1000); ;
            _jobHistoryRepo.Insert(history);
        }

    }
}
