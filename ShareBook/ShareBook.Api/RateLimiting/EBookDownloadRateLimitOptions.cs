namespace ShareBook.Api.RateLimiting;

public sealed class EBookDownloadRateLimitOptions
{
    public const string SectionName = "EBookDownloadRateLimit";

    public int PermitLimit { get; set; } = 5;
    public int WindowHours { get; set; } = 24;
}
