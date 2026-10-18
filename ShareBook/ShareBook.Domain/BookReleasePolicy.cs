using ShareBook.Domain.Enums;
using System;
using System.Linq;
using System.Linq.Expressions;

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

    /// <summary>
    /// Available and already requestable. Excludes scheduled physical books whose release is still in the future.
    /// Use on every public listing, count and recommendation; upcoming books only appear on the home shelf and PDP.
    /// </summary>
    public static Expression<Func<Book, bool>> IsPubliclyAvailable(DateTime utcNow)
        => book => book.Status == BookStatus.Available
            && (book.Type == BookType.Eletronic || book.ReleaseAtUtc == null || book.ReleaseAtUtc <= utcNow);

    public static IQueryable<Book> WherePubliclyAvailable(this IQueryable<Book> query, DateTime utcNow)
        => query.Where(IsPubliclyAvailable(utcNow));

    public static bool IsUpcoming(BookStatus status, BookType type, DateTime? releaseAtUtc, DateTime utcNow)
    {
        if (status != BookStatus.Available || type != BookType.Printed || !releaseAtUtc.HasValue)
            return false;

        if (utcNow.Kind != DateTimeKind.Utc || releaseAtUtc.Value.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Release instants must use UTC.");

        return utcNow < releaseAtUtc.Value;
    }
}
