using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Repository.Repository;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ShareBook.Repository;

public interface ICategoryRepository
{
    IQueryable<Category> Get();

    Task<PagedList<Category>> GetAsync<TKey>(Expression<Func<Category, bool>> filter, Expression<Func<Category, TKey>> order, IncludeList<Category> includes);
}
