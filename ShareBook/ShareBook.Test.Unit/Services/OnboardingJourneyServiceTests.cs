using Microsoft.EntityFrameworkCore;
using ShareBook.Domain;
using ShareBook.Repository;
using ShareBook.Service.Onboarding;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ShareBook.Test.Unit.Services;

public class OnboardingJourneyServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 23, 16, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Enroll_CreatesOnlyOneJourneyForAnEligibleUser()
    {
        await using var context = NewContext();
        var user = NewEligibleUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();
        var service = new OnboardingJourneyService(context, new FixedTimeProvider(Now));

        await service.EnrollAsync(user);
        await service.EnrollAsync(user);

        var journey = await context.OnboardingJourneys.SingleAsync();
        Assert.Equal(user.Id, journey.UserId);
        Assert.Equal(0, journey.NextStep);
        Assert.Equal(Now.UtcDateTime, journey.NextEmailAtUtc);
    }

    [Fact]
    public async Task Enroll_DoesNotCreateJourneyForAnIneligibleUser()
    {
        await using var context = NewContext();
        var user = NewEligibleUser();
        user.AllowSendingEmail = false;
        context.Users.Add(user);
        await context.SaveChangesAsync();

        await new OnboardingJourneyService(context, new FixedTimeProvider(Now)).EnrollAsync(user);

        Assert.Empty(context.OnboardingJourneys);
    }

    [Fact]
    public async Task Cancel_StopsTheActiveJourneyAndPreservesItsHistory()
    {
        await using var context = NewContext();
        var user = NewEligibleUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();
        var service = new OnboardingJourneyService(context, new FixedTimeProvider(Now));
        await service.EnrollAsync(user);

        await service.CancelAsync(user.Id, "unsubscribed");

        var journey = await context.OnboardingJourneys.SingleAsync();
        Assert.Equal(Now.UtcDateTime, journey.CanceledAtUtc);
        Assert.Equal("unsubscribed", journey.CompletionReason);
        Assert.Null(journey.CompletedAtUtc);
    }

    private static ApplicationDbContext NewContext()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static User NewEligibleUser() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Maria",
        Email = "maria@example.com",
        Password = "password",
        PasswordSalt = "salt"
    };

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
