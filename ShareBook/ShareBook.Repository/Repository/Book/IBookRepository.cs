using ShareBook.Domain;

namespace ShareBook.Repository
{
    public interface IBookRepository : IRepositoryGeneric<Book>
    {
        Task<IList<string>> GetSlugsStartingWithAsync(string baseSlug);

        IQueryable<Book> FullTextSearch(string normalizedCriteria, bool includeUnavailable);
    }
}
