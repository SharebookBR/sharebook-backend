using ShareBook.Domain;
using ShareBook.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ShareBook.Repository;

public interface IBookRepository
{
    IQueryable<Book> Get();

    Task<PagedList<Book>> GetAsync<TKey>(Expression<Func<Book, bool>> filter, Expression<Func<Book, TKey>> order);

    Task<Book> UpdateAsync(Book entity);

    Task<IList<string>> GetSlugsStartingWithAsync(string baseSlug);

    IQueryable<Book> FullTextSearch(string normalizedCriteria, bool includeUnavailable);
}
