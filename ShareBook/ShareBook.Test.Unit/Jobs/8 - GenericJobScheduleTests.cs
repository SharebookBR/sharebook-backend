using Microsoft.Extensions.Logging;
using Moq;
using Sharebook.Jobs;
using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Helper;
using ShareBook.Repository;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ShareBook.Test.Unit.Jobs
{
    public class GenericJobScheduleTests
    {
        private readonly Mock<IJobHistoryRepository> _jobHistoryRepo = new();
        private readonly Mock<ILoggerFactory> _loggerFactory = new();

        [Fact]
        public void WeeklyJob_ShouldNotHaveWork_WhenAlreadyExecutedAfterLastScheduledOccurrence()
        {
            var nowSp = DateTimeHelper.GetDateTimeNowSaoPaulo();
            var bestTime = nowSp.TimeOfDay.Add(TimeSpan.FromMinutes(-5));
            if (bestTime < TimeSpan.Zero)
                bestTime = TimeSpan.Zero;

            var lastExecutionUtc = DateTime.UtcNow.AddMinutes(-1);

            _jobHistoryRepo.Setup(x => x.Get()).Returns(new[]
            {
                new JobHistory
                {
                    JobName = "ScheduleAwareFakeJob",
                    IsSuccess = true,
                    CreationDate = lastExecutionUtc
                }
            }.AsQueryable());

            var job = new ScheduleAwareFakeJob(_jobHistoryRepo.Object, _loggerFactory.Object)
            {
                Interval = Interval.Weekly,
                BestDayOfWeek = nowSp.DayOfWeek,
                BestTimeToExecute = bestTime
            };

            Assert.False(job.HasWork());
        }

        [Fact]
        public void WeeklyJob_ShouldHaveWork_WhenLastSuccessfulExecutionIsBeforeCurrentWeeklySlot()
        {
            var nowSp = DateTimeHelper.GetDateTimeNowSaoPaulo();
            var bestTime = nowSp.TimeOfDay.Add(TimeSpan.FromMinutes(-5));
            if (bestTime < TimeSpan.Zero)
                bestTime = TimeSpan.Zero;

            _jobHistoryRepo.Setup(x => x.Get()).Returns(new[]
            {
                new JobHistory
                {
                    JobName = "ScheduleAwareFakeJob",
                    IsSuccess = true,
                    CreationDate = DateTime.UtcNow.AddDays(-8)
                }
            }.AsQueryable());

            var job = new ScheduleAwareFakeJob(_jobHistoryRepo.Object, _loggerFactory.Object)
            {
                Interval = Interval.Weekly,
                BestDayOfWeek = nowSp.DayOfWeek,
                BestTimeToExecute = bestTime
            };

            Assert.True(job.HasWork());
        }

        [Fact]
        public void WeeklyJob_ShouldExposeNextExecutionUsingConfiguredWeekdayAndTime()
        {
            var job = new ScheduleAwareFakeJob(_jobHistoryRepo.Object, _loggerFactory.Object)
            {
                Interval = Interval.Weekly,
                BestDayOfWeek = DayOfWeek.Monday,
                BestTimeToExecute = new TimeSpan(9, 0, 0)
            };

            var nextExecutionUtc = job.GetNextExecutionAtUtc();

            Assert.True(nextExecutionUtc.HasValue);

            var nextExecutionSp = DateTimeHelper.ConvertDateTimeSaoPaulo(nextExecutionUtc.Value);
            Assert.Equal(DayOfWeek.Monday, nextExecutionSp.DayOfWeek);
            Assert.Equal(new TimeSpan(9, 0, 0), nextExecutionSp.TimeOfDay);
            Assert.True(nextExecutionUtc.Value > DateTime.UtcNow);
        }

        private class ScheduleAwareFakeJob : GenericJob, IJob
        {
            public ScheduleAwareFakeJob(IJobHistoryRepository jobHistoryRepo, ILoggerFactory loggerFactory)
                : base(jobHistoryRepo, loggerFactory)
            {
                JobName = "ScheduleAwareFakeJob";
                Description = "Fake job para testar agenda.";
                Active = true;
            }

            public override Task<JobHistory> WorkAsync()
            {
                return Task.FromResult(new JobHistory
                {
                    JobName = JobName,
                    IsSuccess = true
                });
            }
        }
    }
}
