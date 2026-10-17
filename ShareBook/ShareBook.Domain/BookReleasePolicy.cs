using ShareBook.Domain.Enums;
using System;

namespace ShareBook.Domain;

/// <summary>
/// Centralizes release eligibility for scheduled physical book donations.
/// Dates are UTC instants. This policy is deliberately independent of the
/// notification delivery mechanism.
/// </summary>
public static class BookReleasePolicy
{
    public static bool CanRequest(BookStatus status, BookType type, DateTime? releaseAtUtc, DateTime utcNow)
    {
        if (status != BookStatus.Available)
            return false;

        // Existing electronic books are not part of the physical prelaunch feature.
        if (type == BookType.Eletronic)
            return true;

        if (!releaseAtUtc.HasValue)
            return true;

        if (utcNow.Kind != DateTimeKind.Utc || releaseAtUtc.Value.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Release instants must use UTC.");

        return utcNow >= releaseAtUtc.Value;
    }

    public static bool IsUpcoming(BookStatus status, BookType type, DateTime? releaseAtUtc, DateTime utcNow)
    {
        if (status != BookStatus.Available || type != BookType.Printed || !releaseAtUtc.HasValue)
            return false;

        if (utcNow.Kind != DateTimeKind.Utc || releaseAtUtc.Value.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Release instants must use UTC.");

        return utcNow < releaseAtUtc.Value;
    }
}
