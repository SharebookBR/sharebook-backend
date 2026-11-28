using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ShareBook.Domain;
using ShareBook.Repository;
using ShareBook.Service.Onboarding;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ShareBook.Test.Unit.Services;

public class LegacyOnboardingRolloutServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 12, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task SeparatesMatureCohortsAndProjectsTheNextBatch()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = NewContext(connection);
        await context.Database.EnsureCreatedAsync();

        var firstBatch = new LegacyOnboardingBatch
        {
            SequenceNumber = 1,
            ExecutedAtUtc = Now.UtcDateTime.AddDays(-8),
            EnrolledCount = 2
        };
        var secondBatch = new LegacyOnboardingBatch
        {
            SequenceNumber = 2,
            ExecutedAtUtc = Now.UtcDateTime.AddDays(-2),
            EnrolledCount = 2
        };
        var firstUser = User("first@example.com");
        var secondUser = User("second@example.com");
        var remainingUser = User("remaining@example.com");
        var firstJourney = new OnboardingJourney
        {
            UserId = firstUser.Id,
            LegacyOnboardingBatch = firstBatch,
            NextStep = 5,
            NextEmailAtUtc = Now.UtcDateTime,
            CompletedAtUtc = firstBatch.ExecutedAtUtc.AddDays(5)
        };
        var secondJourney = new OnboardingJourney
        {
            UserId = secondUser.Id,
            LegacyOnboardingBatch = secondBatch,
            NextStep = 1,
            NextEmailAtUtc = Now.UtcDateTime
        };
        var firstDelivery = new OnboardingEmailDelivery
        {
            OnboardingJourney = firstJourney,
            Step = 0,
            QueuedAtUtc = firstBatch.ExecutedAtUtc,
            FirstOpenedAtUtc = firstBatch.ExecutedAtUtc.AddHours(1)
        };
        var secondDelivery = new OnboardingEmailDelivery
        {
            OnboardingJourney = secondJourney,
            Step = 0,
            QueuedAtUtc = secondBatch.ExecutedAtUtc
        };

        context.Users.AddRange(firstUser, secondUser, remainingUser);
        context.LegacyOnboardingBatches.AddRange(firstBatch, secondBatch);
        context.OnboardingJourneys.AddRange(firstJourney, secondJourney);
        context.OnboardingEmailDeliveries.AddRange(firstDelivery, secondDelivery);
        context.OnboardingEmailClicks.Add(new OnboardingEmailClick
        {
            OnboardingEmailDelivery = firstDelivery,
            Cta = "catalog",
            FirstClickedAtUtc = firstBatch.ExecutedAtUtc.AddHours(2)
        });
        context.JobHistories.Add(new JobHistory
        {
            JobName = "LegacyOnboardingBackfill",
            IsSuccess = true
        });
        await context.SaveChangesAsync();
        await context.Database.ExecuteSqlRawAsync(
            "UPDATE \"JobHistories\" SET \"CreationDate\" = {0} WHERE \"JobName\" = {1}",
            secondBatch.ExecutedAtUtc,
            "LegacyOnboardingBackfill");
        context.ChangeTracker.Clear();

        var service = new LegacyOnboardingRolloutService(
            context,
            new FixedTimeProvider(Now),
            Options.Create(new LegacyOnboardingBackfillSettings
            {
                BatchSize = 2,
                EligibleBeforeUtc = Now.UtcDateTime.AddDays(-1)
            }));

        var dashboard = await service.GetDashboardAsync();

        Assert.Equal(1, dashboard.RemainingEligibleJourneys);
        Assert.Equal(1, dashboard.RemainingBatches);
        Assert.Equal(secondBatch.ExecutedAtUtc.AddDays(15), dashboard.NextBatchAtUtc);
        Assert.Single(dashboard.UpcomingBatches);
        Assert.Equal(3, dashboard.UpcomingBatches[0].SequenceNumber);
        Assert.Equal(1, dashboard.UpcomingBatches[0].EstimatedJourneys);

        var matureBatch = Assert.Single(dashboard.Batches, batch => batch.SequenceNumber == 1);
        Assert.True(matureBatch.IsComparable);
        Assert.Equal(1, matureBatch.FirstSevenDaysMetrics!.FirstEmailQueued);
        Assert.Equal(1, matureBatch.FirstSevenDaysMetrics.FirstEmailOpened);
        Assert.Equal(1, matureBatch.FirstSevenDaysMetrics.FirstEmailClicked);
        Assert.Equal(1, matureBatch.FirstSevenDaysMetrics.JourneysCompleted);
        Assert.False(Assert.Single(dashboard.Batches, batch => batch.SequenceNumber == 2).IsComparable);
    }

    private static ApplicationDbContext NewContext(SqliteConnection connection)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);

    private static User User(string email) => new()
    {
        Id = Guid.NewGuid(),
        Name = "Maria",
        Email = email,
        Password = "password",
        PasswordSalt = "salt",
        Active = true,
        ParentAproved = true,
        AllowSendingEmail = true,
        CreationDate = Now.UtcDateTime.AddDays(-10)
    };

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
