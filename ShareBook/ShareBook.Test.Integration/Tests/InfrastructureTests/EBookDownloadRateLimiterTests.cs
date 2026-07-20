using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using ShareBook.Api.RateLimiting;
using System.Net;

namespace ShareBook.Test.Integration.Tests.InfrastructureTests;

public class EBookDownloadRateLimiterTests
{
    [Fact]
    public void SixthDownloadFromSameIp_IsRejected()
    {
        var (limiter, _) = CreateLimiter();
        var clientIp = IPAddress.Parse("203.0.113.25");

        for (var attempt = 1; attempt <= 5; attempt++)
        {
            var result = limiter.TryAcquire(clientIp);
            result.IsAllowed.Should().BeTrue();
            result.Remaining.Should().Be(5 - attempt);
        }

        var rejected = limiter.TryAcquire(clientIp);

        rejected.IsAllowed.Should().BeFalse();
        rejected.Remaining.Should().Be(0);
        rejected.RetryAfterSeconds.Should().Be(24 * 60 * 60);
    }

    [Fact]
    public void DifferentIp_HasIndependentLimit()
    {
        var (limiter, _) = CreateLimiter();
        var firstClientIp = IPAddress.Parse("203.0.113.25");
        var secondClientIp = IPAddress.Parse("198.51.100.20");

        for (var attempt = 0; attempt < 5; attempt++)
            limiter.TryAcquire(firstClientIp).IsAllowed.Should().BeTrue();

        limiter.TryAcquire(firstClientIp).IsAllowed.Should().BeFalse();
        limiter.TryAcquire(secondClientIp).IsAllowed.Should().BeTrue();
    }

    [Fact]
    public void LimitExpiresAfterTwentyFourHours()
    {
        var (limiter, timeProvider) = CreateLimiter();
        var clientIp = IPAddress.Parse("203.0.113.25");

        for (var attempt = 0; attempt < 5; attempt++)
            limiter.TryAcquire(clientIp).IsAllowed.Should().BeTrue();

        limiter.TryAcquire(clientIp).IsAllowed.Should().BeFalse();

        timeProvider.Advance(TimeSpan.FromHours(24));

        var result = limiter.TryAcquire(clientIp);
        result.IsAllowed.Should().BeTrue();
        result.Remaining.Should().Be(4);
    }

    [Fact]
    public void Ipv4MappedAddress_SharesTheSameLimit()
    {
        var (limiter, _) = CreateLimiter();
        var ipv4 = IPAddress.Parse("203.0.113.25");
        var mappedIpv4 = ipv4.MapToIPv6();

        for (var attempt = 0; attempt < 5; attempt++)
            limiter.TryAcquire(ipv4).IsAllowed.Should().BeTrue();

        limiter.TryAcquire(mappedIpv4).IsAllowed.Should().BeFalse();
    }

    private static (EBookDownloadRateLimiter Limiter, ManualTimeProvider TimeProvider)
        CreateLimiter()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var timeProvider = new ManualTimeProvider(DateTimeOffset.UtcNow);
        var options = Options.Create(new EBookDownloadRateLimitOptions
        {
            PermitLimit = 5,
            WindowHours = 24
        });

        return (
            new EBookDownloadRateLimiter(cache, timeProvider, options),
            timeProvider);
    }

    private sealed class ManualTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        private DateTimeOffset _utcNow = utcNow;

        public override DateTimeOffset GetUtcNow() => _utcNow;

        public void Advance(TimeSpan duration) => _utcNow = _utcNow.Add(duration);
    }
}
