using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Repository.Repository;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ShareBook.Repository;

public class CategoryRepository(ApplicationDbContext context) : ICategoryRepository
{
    private readonly EntityCrud<Category> _crud = new EntityCrud<Category>(context);

    public IQueryable<Category> Get() => _crud.Get();

    public Task<PagedList<Category>> GetAsync<TKey>(Expression<Func<Category, bool>> filter, Expression<Func<Category, TKey>> order, IncludeList<Category> includes)
        => _crud.GetAsync(filter, order, includes);
}
