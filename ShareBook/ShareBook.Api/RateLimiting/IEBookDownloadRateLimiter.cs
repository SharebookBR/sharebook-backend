using System.Net;

namespace ShareBook.Api.RateLimiting;

public interface IEBookDownloadRateLimiter
{
    EBookDownloadRateLimitResult TryAcquire(IPAddress clientIp);
}

public readonly record struct EBookDownloadRateLimitResult(
    bool IsAllowed,
    int Remaining,
    int RetryAfterSeconds);
