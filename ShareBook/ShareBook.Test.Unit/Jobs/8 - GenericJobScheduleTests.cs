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

namespace ShareBook.Test.Unit.Jobs;

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

    [Fact]
    public void GetDateLimitByInterval_UsesInjectedTimeProvider_NotTheRealClock()
    {
        // Prova que dá pra controlar o relógio em teste (Tarefa 10): antes do TimeProvider,
        // GetDateLimitByInterval usava DateTime.UtcNow direto e não tinha como ser testado
        // de forma determinística sem depender da hora real da máquina rodando o teste.
        var fixedNow = new DateTimeOffset(2026, 3, 15, 10, 0, 0, TimeSpan.Zero);
        var fakeTimeProvider = new FixedTimeProvider(fixedNow);

        var job = new ScheduleAwareFakeJob(_jobHistoryRepo.Object, _loggerFactory.Object, fakeTimeProvider)
        {
            Interval = Interval.Hourly
        };

        var dateLimit = job.GetDateLimitByInterval(Interval.Hourly);

        // GetDateLimitByInterval soma +1 minuto de folga além do intervalo (ver implementação),
        // pra dar margem ao tempo que o próprio job leva pra rodar.
        Assert.Equal(fixedNow.UtcDateTime.AddHours(-1).AddMinutes(1), dateLimit);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private class ScheduleAwareFakeJob : GenericJob, IJob
    {
        public ScheduleAwareFakeJob(IJobHistoryRepository jobHistoryRepo, ILoggerFactory loggerFactory)
            : base(jobHistoryRepo, loggerFactory, TimeProvider.System)
        {
            JobName = "ScheduleAwareFakeJob";
            Description = "Fake job para testar agenda.";
            Active = true;
        }

        public ScheduleAwareFakeJob(IJobHistoryRepository jobHistoryRepo, ILoggerFactory loggerFactory, TimeProvider timeProvider)
            : base(jobHistoryRepo, loggerFactory, timeProvider)
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
