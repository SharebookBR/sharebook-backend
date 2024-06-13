using ShareBook.Domain.Common;
using ShareBook.Repository.Repository;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ShareBook.Repository
{
    public interface IRepositoryGeneric<TEntity> where TEntity : class
    {
        Task<TEntity> FindAsync(params object[] keyValues);

        Task<TEntity> FindAsync(IncludeList<TEntity> includes, params object[] keyValues);

        Task<TEntity> FindAsync(IncludeList<TEntity> includes, Expression<Func<TEntity, bool>> filter);

        Task<PagedList<TEntity>> GetAsync<TKey>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> order, int page, int itemsPerPage);

        Task<PagedList<TEntity>> GetAsync<TKey>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> order, int page, int itemsPerPage, IncludeList<TEntity> includes);

        Task<int> CountAsync(Expression<Func<TEntity, bool>> filter);

        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter);

        Task<TEntity> InsertAsync(TEntity entity);

        Task<TEntity> UpdateAsync(TEntity entity);

        Task DeleteAsync(params object[] keyValues);

        Task DeleteAsync(TEntity entity);

        /// <summary>
        /// Get the DbSet as a IQueryable
        /// </summary>
        IQueryable<TEntity> Get();

        /// <summary>
        /// Find in the DbSet an entity that matches the specified filter.
        /// </summary>
        /// <returns>Entity with the child objects</returns>
        /// <exception cref="ShareBook.Domain.Exceptions.ShareBookException">
        /// In case that more than 1 entity could be returned for the filter specified.
        /// </exception>
        TEntity Find(Expression<Func<TEntity, bool>> filter);

        /// <summary>
        /// Get ALL the entities based on the filter passed, on the specified order, without child objects.
        /// </summary>
        /// <returns>PageList with only 1 page and all the items</returns>
        PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> order);

        /// <summary>
        /// Get ALL the entities based on the filter passed, on the specified order, with the
        /// specified child objects.
        /// </summary>
        /// <returns>PageList with only 1 page and all the items</returns>
        PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> order, IncludeList<TEntity> includes);

        /// <summary>
        /// Get a paged list of the entity, without any filter, on the specified order, with the
        /// specified child objects.
        /// </summary>
        /// <param name="page">First Page = 1</param>
        PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, TKey>> order, int page, int itemsPerPage, IncludeList<TEntity> includes);

        /// <summary>
        /// Get a paged list of the entity, based on the filter passed, on the specified order,
        /// without child objects.
        /// </summary>
        /// <param name="page">First Page = 1</param>
        PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> order, int page, int itemsPerPage);

        /// <summary>
        /// Get a paged list of the entity, based on the filter passed, on the specified order, with
        /// the specified child objects.
        /// </summary>
        /// <param name="page">First Page = 1</param>
        PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> order, int page, int itemsPerPage, IncludeList<TEntity> includes);

        IQueryable<TEntity> FromSql(string query, object[] parameters);
    }
}