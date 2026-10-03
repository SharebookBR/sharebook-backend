using ShareBook.Domain;
using ShareBook.Domain.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareBook.Service;

public interface ITagService
{
    Task<IList<Tag>> GetPublicTagsAsync();
    Task<IDictionary<string, int>> GetPublicBookCountsByTagAsync(IEnumerable<string> tagIds);
    Task<IList<Tag>> GetAdminTagsAsync();
    Task<Tag?> FindPublicAsync(string idOrAlias);
    Task<Tag?> FindAdminAsync(string id);
    Task<PagedList<Book>> GetPublicBooksAsync(string idOrAlias, int page, int itemsPerPage);
    Task<Tag> CreateAsync(Tag tag);
    Task<Tag> UpdateAsync(string id, Tag tag);
    Task DeprecateAsync(string id);
    Task<IList<BookTag>> GetBookTagsAsync(Guid bookId);
    Task<IList<BookTag>> SetBookTagsAsync(Guid bookId, IEnumerable<string> tagIds);
    Task<IList<BookTag>> ApplyMechanicalTagsAsync(Guid bookId, string title, string? synopsis);
}
