using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net;

namespace ShareBook.Api.RateLimiting;

public sealed class EBookDownloadRateLimiter : IEBookDownloadRateLimiter
{
    private const string CacheKeyPrefix = "ebook-download-rate-limit:";

    private readonly IMemoryCache _cache;
    private readonly TimeProvider _timeProvider;
    private readonly int _permitLimit;
    private readonly TimeSpan _window;
    private readonly object _sync = new();

    public EBookDownloadRateLimiter(
        IMemoryCache cache,
        TimeProvider timeProvider,
        IOptions<EBookDownloadRateLimitOptions> options)
    {
        _cache = cache;
        _timeProvider = timeProvider;
        _permitLimit = options.Value.PermitLimit;
        _window = TimeSpan.FromHours(options.Value.WindowHours);
    }

    public EBookDownloadRateLimitResult TryAcquire(IPAddress clientIp)
    {
        var now = _timeProvider.GetUtcNow();
        var cacheKey = CacheKeyPrefix + Normalize(clientIp);

        lock (_sync)
        {
            if (_cache.TryGetValue(cacheKey, out RateLimitEntry entry)
                && entry.ResetAt > now)
            {
                if (entry.Count >= _permitLimit)
                    return Denied(entry.ResetAt, now);

                var updatedEntry = entry with { Count = entry.Count + 1 };
                _cache.Set(cacheKey, updatedEntry, entry.ResetAt);

                return new EBookDownloadRateLimitResult(
                    IsAllowed: true,
                    Remaining: _permitLimit - updatedEntry.Count,
                    RetryAfterSeconds: 0);
            }

            var resetAt = now.Add(_window);
            _cache.Set(cacheKey, new RateLimitEntry(1, resetAt), resetAt);

            return new EBookDownloadRateLimitResult(
                IsAllowed: true,
                Remaining: _permitLimit - 1,
                RetryAfterSeconds: 0);
        }
    }

    private static string Normalize(IPAddress clientIp)
    {
        if (clientIp == null)
            return "unknown";

        return clientIp.IsIPv4MappedToIPv6
            ? clientIp.MapToIPv4().ToString()
            : clientIp.ToString();
    }

    private static EBookDownloadRateLimitResult Denied(
        DateTimeOffset resetAt,
        DateTimeOffset now)
    {
        var retryAfterSeconds = Math.Max(
            1,
            (int)Math.Ceiling((resetAt - now).TotalSeconds));

        return new EBookDownloadRateLimitResult(
            IsAllowed: false,
            Remaining: 0,
            RetryAfterSeconds: retryAfterSeconds);
    }

    private readonly record struct RateLimitEntry(
        int Count,
        DateTimeOffset ResetAt);
}
