using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Data.Sqlite;
using Moq;
using Sharebook.Jobs;
using ShareBook.Domain;
using ShareBook.Repository;
using ShareBook.Service.Onboarding;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ShareBook.Test.Unit.Jobs;

public class LegacyOnboardingBackfillTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 12, 3, 30, 0, TimeSpan.Zero);
    private static readonly DateTime LegacyCutoff = new(2026, 10, 23, 18, 26, 0, DateTimeKind.Utc);

    [Fact]
    public async Task CreatesBatchesNewestFirstAndContinuesFromTheSavedCursor()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = NewContext(connection);
        await context.Database.EnsureCreatedAsync();
        var newest = NewUser("newest@example.com", LegacyCutoff.AddMinutes(-1));
        var second = NewUser("second@example.com", LegacyCutoff.AddMinutes(-2));
        var invalidEmail = NewUser("not-an-email", LegacyCutoff.AddMinutes(-3));
        var third = NewUser("third@example.com", LegacyCutoff.AddMinutes(-4));
        var bounced = NewUser("bounced@example.com", LegacyCutoff.AddMinutes(-5));
        var fourth = NewUser("fourth@example.com", LegacyCutoff.AddMinutes(-6));
        var optedOut = NewUser("optedout@example.com", LegacyCutoff.AddMinutes(-7));
        optedOut.AllowSendingEmail = false;
        var alreadyEnrolled = NewUser("already@example.com", LegacyCutoff.AddMinutes(-8));

        context.Users.AddRange(newest, second, invalidEmail, third, bounced, fourth, optedOut, alreadyEnrolled);
        context.MailBounces.Add(new MailBounce(string.Empty, string.Empty) { Email = bounced.Email, IsSoft = false });
        context.OnboardingJourneys.Add(new OnboardingJourney { UserId = alreadyEnrolled.Id, NextStep = 1, NextEmailAtUtc = Now.UtcDateTime });
        await context.SaveChangesAsync();

        await SetCreationDateAsync(context, newest.Id, LegacyCutoff.AddMinutes(-1));
        await SetCreationDateAsync(context, second.Id, LegacyCutoff.AddMinutes(-2));
        await SetCreationDateAsync(context, invalidEmail.Id, LegacyCutoff.AddMinutes(-3));
        await SetCreationDateAsync(context, third.Id, LegacyCutoff.AddMinutes(-4));
        await SetCreationDateAsync(context, bounced.Id, LegacyCutoff.AddMinutes(-5));
        await SetCreationDateAsync(context, fourth.Id, LegacyCutoff.AddMinutes(-6));
        await SetCreationDateAsync(context, optedOut.Id, LegacyCutoff.AddMinutes(-7));
        await SetCreationDateAsync(context, alreadyEnrolled.Id, LegacyCutoff.AddMinutes(-8));
        context.ChangeTracker.Clear();

        var historyRepository = new Mock<IJobHistoryRepository>();
        historyRepository.Setup(repository => repository.Get()).Returns(() => context.JobHistories);
        var firstJob = CreateJob(context, historyRepository.Object);

        var firstResult = await firstJob.WorkAsync();
        Assert.Contains("2 jornada", firstResult.Details);
        Assert.Equal(2, await context.OnboardingJourneys.CountAsync(journey => journey.NextStep == 0));
        Assert.Equal(new[] { newest.Id, second.Id }, await context.OnboardingJourneys
            .Where(journey => journey.NextStep == 0)
            .OrderByDescending(journey => journey.User.CreationDate)
            .Select(journey => journey.UserId)
            .ToArrayAsync());

        context.JobHistories.Add(firstResult);
        await context.SaveChangesAsync();

        var secondResult = await CreateJob(context, historyRepository.Object).WorkAsync();
        Assert.Contains("2 jornada", secondResult.Details);
        var legacyJourneyIds = await context.OnboardingJourneys
            .Where(journey => journey.NextStep == 0)
            .Select(journey => journey.UserId)
            .ToListAsync();
        Assert.Contains(third.Id, legacyJourneyIds);
        Assert.Contains(fourth.Id, legacyJourneyIds);
        Assert.DoesNotContain(invalidEmail.Id, legacyJourneyIds);
        Assert.DoesNotContain(bounced.Id, legacyJourneyIds);
        Assert.DoesNotContain(optedOut.Id, legacyJourneyIds);
    }

    [Fact]
    public async Task DoesNotIncludeUsersCreatedAfterOnboardingWasLaunched()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = NewContext(connection);
        await context.Database.EnsureCreatedAsync();
        var legacyUser = NewUser("legacy@example.com", LegacyCutoff.AddSeconds(-1));
        var recentUser = NewUser("recent@example.com", LegacyCutoff.AddSeconds(1));
        context.Users.AddRange(legacyUser, recentUser);
        await context.SaveChangesAsync();
        await SetCreationDateAsync(context, legacyUser.Id, LegacyCutoff.AddSeconds(-1));
        await SetCreationDateAsync(context, recentUser.Id, LegacyCutoff.AddSeconds(1));
        context.ChangeTracker.Clear();

        var historyRepository = new Mock<IJobHistoryRepository>();
        historyRepository.Setup(repository => repository.Get()).Returns(() => context.JobHistories);

        await CreateJob(context, historyRepository.Object).WorkAsync();

        var journey = await context.OnboardingJourneys.SingleAsync();
        Assert.Equal(legacyUser.Id, journey.UserId);
    }

    private static LegacyOnboardingBackfill CreateJob(ApplicationDbContext context, IJobHistoryRepository historyRepository)
        => new(
            historyRepository,
            NullLoggerFactory.Instance,
            new FixedTimeProvider(Now),
            context,
            Options.Create(new LegacyOnboardingBackfillSettings
            {
                BatchSize = 2,
                EligibleBeforeUtc = LegacyCutoff
            }));

    private static ApplicationDbContext NewContext(SqliteConnection connection)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options);

    private static User NewUser(string email, DateTime creationDate) => new()
    {
        Id = Guid.NewGuid(),
        Name = "Maria",
        Email = email,
        Password = "password",
        PasswordSalt = "salt",
        Active = true,
        ParentAproved = true,
        AllowSendingEmail = true,
        CreationDate = creationDate
    };

    private static Task SetCreationDateAsync(ApplicationDbContext context, Guid userId, DateTime creationDate)
        => context.Database.ExecuteSqlRawAsync(
            "UPDATE \"Users\" SET \"CreationDate\" = {0} WHERE \"Id\" = {1}",
            creationDate,
            userId);

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
