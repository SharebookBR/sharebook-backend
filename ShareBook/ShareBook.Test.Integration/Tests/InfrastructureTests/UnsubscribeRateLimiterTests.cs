using Microsoft.Extensions.Caching.Memory;
using ShareBook.Api.RateLimiting;

namespace ShareBook.Test.Integration.Tests.InfrastructureTests;

public class UnsubscribeRateLimiterTests
{
    [Fact]
    public void SecondRequestWithinFiveSeconds_IsRejectedGlobally()
    {
        var (limiter, _) = CreateLimiter();

        limiter.TryAcquire().IsAllowed.Should().BeTrue();

        var blocked = limiter.TryAcquire();

        blocked.IsAllowed.Should().BeFalse();
        blocked.RetryAfterSeconds.Should().Be(5);
    }

    [Fact]
    public void RequestAfterFiveSeconds_IsAllowedAgain()
    {
        var (limiter, timeProvider) = CreateLimiter();
        limiter.TryAcquire().IsAllowed.Should().BeTrue();
        limiter.TryAcquire().IsAllowed.Should().BeFalse();

        timeProvider.Advance(TimeSpan.FromSeconds(5));

        var allowedAgain = limiter.TryAcquire();

        allowedAgain.IsAllowed.Should().BeTrue();
        allowedAgain.RetryAfterSeconds.Should().Be(0);
    }

    private static (UnsubscribeRateLimiter Limiter, ManualTimeProvider TimeProvider)
        CreateLimiter()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var timeProvider = new ManualTimeProvider(DateTimeOffset.UtcNow);
        return (new UnsubscribeRateLimiter(cache, timeProvider), timeProvider);
    }

    private sealed class ManualTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        private DateTimeOffset _utcNow = utcNow;

        public override DateTimeOffset GetUtcNow() => _utcNow;

        public void Advance(TimeSpan duration) => _utcNow = _utcNow.Add(duration);
    }
}
