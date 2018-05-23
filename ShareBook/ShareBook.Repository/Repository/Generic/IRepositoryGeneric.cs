using ShareBook.Domain.Common;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ShareBook.Repository
{
    public interface IRepositoryGeneric<TEntity> where TEntity : class
    {
        TEntity Get(params object[] keyValues);
        IQueryable<TEntity> Get();
        PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> order, int page, int itemsPerPage);
        int Count(Expression<Func<TEntity, bool>> filter);
        bool Any(Expression<Func<TEntity, bool>> filter);
        TEntity Insert(TEntity entity);
        TEntity Update(TEntity entity);
        void Delete(params object[] keyValues);
        void Delete(TEntity entity);

        Task<TEntity> GetAsync(params object[] keyValues);
        Task<PagedList<TEntity>> GetAsync<TKey>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> order, int page, int itemsPerPage);
        Task<int> CountAsync(Expression<Func<TEntity, bool>> filter);
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter);
        Task<TEntity> InsertAsync(TEntity entity);
        Task<TEntity> UpdateAsync(TEntity entity);
        Task DeleteAsync(params object[] keyValues);
        Task DeleteAsync(TEntity entity);
    }
}
