using Microsoft.Extensions.Logging;
using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Domain.Exceptions;
using ShareBook.Helper;
using ShareBook.Repository;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Sharebook.Jobs
{
    public abstract class GenericJob
    {
        public string JobName { get; set; }
        public string Description { get; set; }
        public Interval Interval { get; set; }
        public bool Active { get; set; }
        public DayOfWeek? BestDayOfWeek { get; set; }
        public TimeSpan? BestTimeToExecute { get; set; }

        protected readonly IJobHistoryRepository _jobHistoryRepo;
        protected readonly ILogger Logger;

        protected Stopwatch _stopwatch;

        protected GenericJob(IJobHistoryRepository jobHistoryRepo, ILoggerFactory loggerFactory)
        {
            _jobHistoryRepo = jobHistoryRepo;
            Logger = loggerFactory.CreateLogger(GetType().Name);
        }

        public bool HasWork()
        {
            var lastExpectedExecutionAtUtc = GetLastExpectedExecutionAtUtc();

            if (lastExpectedExecutionAtUtc.HasValue)
            {
                var lastSuccessfulExecutionAtUtc = _jobHistoryRepo.Get()
                    .Where(x => x.JobName == JobName && x.IsSuccess)
                    .OrderByDescending(x => x.CreationDate)
                    .Select(x => x.CreationDate)
                    .FirstOrDefault();

                return !lastSuccessfulExecutionAtUtc.HasValue || lastSuccessfulExecutionAtUtc.Value < lastExpectedExecutionAtUtc.Value;
            }

            var dateLimit = GetDateLimitByInterval(Interval);

            var hasHistory = _jobHistoryRepo.Get()
                .Where(
                    x => x.CreationDate > dateLimit &&
                    x.JobName == JobName &&
                    x.IsSuccess)
                .Any();

            return !hasHistory;
        }

        public DateTime? GetNextExecutionAtUtc()
        {
            var nowSp = DateTimeHelper.GetDateTimeNowSaoPaulo();

            return Interval switch
            {
                Interval.Dayly when BestTimeToExecute.HasValue
                    => DateTimeHelper.ConvertDateTimeToUtcFromSaoPaulo(GetNextDailyOccurrence(nowSp)),
                Interval.Weekly when BestTimeToExecute.HasValue && BestDayOfWeek.HasValue
                    => DateTimeHelper.ConvertDateTimeToUtcFromSaoPaulo(GetNextWeeklyOccurrence(nowSp)),
                _ => null
            };
        }

        public DateTime GetDateLimitByInterval(Interval i)
        {
            var result = DateTime.UtcNow;

            switch (i)
            {
                case Interval.Dayly:
                {
                    result = result.AddHours(-23);
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

        private DateTime? GetLastExpectedExecutionAtUtc()
        {
            var nowSp = DateTimeHelper.GetDateTimeNowSaoPaulo();

            return Interval switch
            {
                Interval.Dayly when BestTimeToExecute.HasValue
                    => DateTimeHelper.ConvertDateTimeToUtcFromSaoPaulo(GetLastDailyOccurrence(nowSp)),
                Interval.Weekly when BestTimeToExecute.HasValue && BestDayOfWeek.HasValue
                    => DateTimeHelper.ConvertDateTimeToUtcFromSaoPaulo(GetLastWeeklyOccurrence(nowSp)),
                _ => null
            };
        }

        private DateTime GetLastDailyOccurrence(DateTime nowSp)
        {
            var todayAtBestTime = nowSp.Date.Add(BestTimeToExecute!.Value);
            return nowSp >= todayAtBestTime ? todayAtBestTime : todayAtBestTime.AddDays(-1);
        }

        private DateTime GetNextDailyOccurrence(DateTime nowSp)
        {
            var todayAtBestTime = nowSp.Date.Add(BestTimeToExecute!.Value);
            return nowSp < todayAtBestTime ? todayAtBestTime : todayAtBestTime.AddDays(1);
        }

        private DateTime GetLastWeeklyOccurrence(DateTime nowSp)
        {
            var daysSinceBestDay = ((7 + (int)nowSp.DayOfWeek - (int)BestDayOfWeek!.Value) % 7);
            var candidate = nowSp.Date.AddDays(-daysSinceBestDay).Add(BestTimeToExecute!.Value);
            return candidate <= nowSp ? candidate : candidate.AddDays(-7);
        }

        private DateTime GetNextWeeklyOccurrence(DateTime nowSp)
        {
            var daysUntilBestDay = ((7 + (int)BestDayOfWeek!.Value - (int)nowSp.DayOfWeek) % 7);
            var candidate = nowSp.Date.AddDays(daysUntilBestDay).Add(BestTimeToExecute!.Value);
            return candidate > nowSp ? candidate : candidate.AddDays(7);
        }

        public async Task<JobResult> ExecuteAsync()
        {
            try {
                BeforeWork();
                var history = await WorkAsync();
                await AfterWorkAsync(history);

                return JobResult.Success;
            }
            catch (AwsSqsDisabledException)
            {
                return JobResult.AwsSqsDisabled;
            }
            catch (MeetupDisabledException)
            {
                return JobResult.MeetupDisabled;
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, "Job {JobName} falhou", JobName);
                return JobResult.Error;
            }

        }

        public abstract Task<JobHistory> WorkAsync();

        protected void BeforeWork()
        {
            _stopwatch = Stopwatch.StartNew();
        }

        protected async Task AfterWorkAsync(JobHistory history)
        {
            _stopwatch.Stop();

            history.TimeSpentSeconds = ((double)_stopwatch.ElapsedMilliseconds / (double)1000);
            await _jobHistoryRepo.InsertAsync(history);
        }

    }
}
