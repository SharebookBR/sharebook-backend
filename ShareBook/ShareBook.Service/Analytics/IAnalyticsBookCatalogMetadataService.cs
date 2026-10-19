using ShareBook.Domain.Enums;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ShareBook.Service.Analytics;

public interface IAnalyticsBookCatalogMetadataService
{
    Task<IReadOnlyDictionary<string, AnalyticsBookCatalogMetadata>> GetBySlugAsync(
        IEnumerable<string> slugs,
        CancellationToken cancellationToken = default);
}

public sealed record AnalyticsBookCatalogMetadata(BookType Type, BookStatus Status);
