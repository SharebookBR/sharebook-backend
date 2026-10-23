using Microsoft.Extensions.Caching.Memory;

namespace ShareBook.Api.RateLimiting;

/// <summary>
/// A short, application-wide cooldown for the public unsubscribe confirmation.
/// It deliberately does not vary by IP or account: this endpoint has very low
/// expected volume and must remain easy to understand and operate.
/// </summary>
public sealed class UnsubscribeRateLimiter(
    IMemoryCache cache,
    TimeProvider timeProvider) : IUnsubscribeRateLimiter
{
    private const string CacheKey = "unsubscribe-rate-limit";
    private static readonly TimeSpan Window = TimeSpan.FromSeconds(5);

    private readonly IMemoryCache _cache = cache;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly object _sync = new();

    public UnsubscribeRateLimitResult TryAcquire()
    {
        var now = _timeProvider.GetUtcNow();

        lock (_sync)
        {
            if (_cache.TryGetValue(CacheKey, out DateTimeOffset nextAllowedAt)
                && nextAllowedAt > now)
            {
                var retryAfterSeconds = Math.Max(
                    1,
                    (int)Math.Ceiling((nextAllowedAt - now).TotalSeconds));

                return new UnsubscribeRateLimitResult(false, retryAfterSeconds);
            }

            var resetAt = now.Add(Window);
            _cache.Set(CacheKey, resetAt, resetAt);
            return new UnsubscribeRateLimitResult(true, 0);
        }
    }
}
