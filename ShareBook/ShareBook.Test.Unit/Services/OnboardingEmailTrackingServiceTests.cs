using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ShareBook.Domain;
using ShareBook.Repository;
using ShareBook.Service.Onboarding;
using ShareBook.Service.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ShareBook.Test.Unit.Services;

public class OnboardingEmailTrackingServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 12, 1, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task RecordsFirstOpenAndFirstClickOnlyForTheExpectedCta()
    {
        await using var context = NewContext();
        var journey = new OnboardingJourney { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), NextEmailAtUtc = Now.UtcDateTime };
        var delivery = new OnboardingEmailDelivery { Id = Guid.NewGuid(), OnboardingJourneyId = journey.Id, Step = 2, QueuedAtUtc = Now.UtcDateTime };
        context.AddRange(journey, delivery);
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var token = service.BuildOpenUrl(delivery.Id).Split('/').Last();
        await service.RecordOpenAsync(token);
        await service.RecordOpenAsync(token);
        var destination = await service.RecordClickAndGetDestinationAsync(token, "donate");
        await service.RecordClickAndGetDestinationAsync(token, "donate");
        var invalidCta = await service.RecordClickAndGetDestinationAsync(token, "catalog");

        var saved = await context.OnboardingEmailDeliveries.SingleAsync();
        Assert.Equal(Now.UtcDateTime, saved.FirstOpenedAtUtc);
        Assert.Equal("https://www.sharebook.com.br/livros/doar", destination);
        Assert.Null(invalidCta);
        Assert.Single(await context.OnboardingEmailClicks.ToListAsync());
    }

    [Fact]
    public async Task IgnoresTokenWithInvalidSignature()
    {
        await using var context = NewContext();
        var service = CreateService(context);

        await service.RecordOpenAsync("not-a-valid-token");

        Assert.Empty(await context.OnboardingEmailDeliveries.ToListAsync());
    }

    private static OnboardingEmailTrackingService CreateService(ApplicationDbContext context) => new(
        context,
        new ConfigurationBuilder().AddInMemoryCollection(new[]
        {
            new KeyValuePair<string, string?>("ServerSettings:OnboardingTrackingSecret", "test-tracking-secret")
        }).Build(),
        Options.Create(new ServerSettings { BackendUrl = "https://api.sharebook.com.br", FrontendUrl = "https://www.sharebook.com.br" }),
        Options.Create(new OnboardingSettings { CommunityUrl = "https://chat.whatsapp.com/invite" }),
        new FixedTimeProvider(Now));

    private static ApplicationDbContext NewContext() => new(new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
