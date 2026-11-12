using Microsoft.EntityFrameworkCore;
using ShareBook.Domain;
using ShareBook.Repository;
using ShareBook.Service.Onboarding;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ShareBook.Test.Unit.Services;

public class OnboardingAnalyticsServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 12, 1, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task BuildsFunnelFromDeliveriesWithoutDependingOnGa4()
    {
        await using var context = NewContext();
        var journey = new OnboardingJourney
        {
            Id = Guid.NewGuid(), UserId = Guid.NewGuid(), CreationDate = Now.AddDays(-2).UtcDateTime,
            NextEmailAtUtc = Now.AddDays(1).UtcDateTime, CompletedAtUtc = Now.AddHours(-1).UtcDateTime
        };
        var delivery = new OnboardingEmailDelivery
        {
            Id = Guid.NewGuid(), OnboardingJourneyId = journey.Id, Step = 0,
            QueuedAtUtc = Now.AddDays(-2).UtcDateTime, FirstOpenedAtUtc = Now.AddDays(-1).UtcDateTime
        };
        var click = new OnboardingEmailClick
        {
            OnboardingEmailDeliveryId = delivery.Id, Cta = "catalog", FirstClickedAtUtc = Now.AddDays(-1).UtcDateTime
        };
        context.AddRange(journey, delivery, click);
        await context.SaveChangesAsync();

        var dashboard = await new OnboardingAnalyticsService(context, new FixedTimeProvider(Now)).GetDashboardAsync(7);

        Assert.Equal(1, dashboard.JourneysStarted);
        Assert.Equal(1, dashboard.JourneysCompleted);
        Assert.Equal(0, dashboard.OverdueJourneys);
        var welcome = dashboard.Steps.Single(step => step.Step == 0);
        Assert.Equal(1, welcome.Queued);
        Assert.Equal(1, welcome.Opened);
        Assert.Equal(1, welcome.Clicked);
        Assert.Equal("catalog", Assert.Single(welcome.Ctas).Cta);
    }

    private static ApplicationDbContext NewContext() => new(new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
