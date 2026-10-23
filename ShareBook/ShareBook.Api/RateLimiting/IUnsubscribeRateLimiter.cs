namespace ShareBook.Api.RateLimiting;

public interface IUnsubscribeRateLimiter
{
    UnsubscribeRateLimitResult TryAcquire();
}

public readonly record struct UnsubscribeRateLimitResult(
    bool IsAllowed,
    int RetryAfterSeconds);
