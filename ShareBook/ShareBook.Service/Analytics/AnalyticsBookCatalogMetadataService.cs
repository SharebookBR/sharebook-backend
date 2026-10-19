using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShareBook.Repository;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShareBook.Service.Analytics;

/// <summary>
/// Batch-loads the current catalog metadata needed by analytics. The analytics
/// service is a singleton, so the DbContext is deliberately resolved inside a
/// short-lived scope rather than injected into it.
/// </summary>
public sealed class AnalyticsBookCatalogMetadataService(IServiceScopeFactory scopeFactory)
    : IAnalyticsBookCatalogMetadataService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    public async Task<IReadOnlyDictionary<string, AnalyticsBookCatalogMetadata>> GetBySlugAsync(
        IEnumerable<string> slugs,
        CancellationToken cancellationToken = default)
    {
        var requestedSlugs = slugs
            .Where(slug => !string.IsNullOrWhiteSpace(slug))
            .Distinct()
            .ToArray();

        if (requestedSlugs.Length == 0)
            return new Dictionary<string, AnalyticsBookCatalogMetadata>();

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.Books
            .AsNoTracking()
            .Where(book => book.Slug != null && requestedSlugs.Contains(book.Slug))
            .Select(book => new
            {
                Slug = book.Slug!,
                book.Type,
                book.Status
            })
            .ToDictionaryAsync(
                book => book.Slug,
                book => new AnalyticsBookCatalogMetadata(book.Type, book.Status),
                cancellationToken);
    }
}
