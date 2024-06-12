using ShareBook.Domain.Common;
using ShareBook.Repository.Repository;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ShareBook.Service.Generic
{
    public interface IBaseService<TEntity> where TEntity : class
    {
        bool Any(Expression<Func<TEntity, bool>> filter);
        Task<int> CountAsync(Expression<Func<TEntity, bool>> filter);

        /// <summary>
        /// Execute a Find on the DbSet using the <paramref name="keyValues"/>.
        /// <para>Using this method will return onlye the Entity, without the children.</para>
        /// <para>If you want to get the Child Objects too, use the <seealso cref="Find(IncludeList{TEntity}, object[])"/> method.</para>
        /// </summary>
        Task<TEntity> FindAsync(object keyValue);

        /// <summary>
        /// Execute a Find on the DbSet using the <paramref name="keyValues"/> and the <paramref name="includes"/>
        /// </summary>
        Task<TEntity> FindAsync(IncludeList<TEntity> includes, object keyValue);

        /// <summary>
        /// Find in the DbSet an entity that matches the specified filter.
        /// </summary>
        /// <returns>Entity with the child objects</returns>
        /// <exception cref="ShareBook.Domain.Exceptions.ShareBookException">In case that more than 1 entity could be returned for the filter specified.</exception>
        TEntity Find(Expression<Func<TEntity, bool>> filter);

        /// <summary>
        /// Find in the DbSet an entity that matches the specified filter.
        /// </summary>
        /// <param name="includes">Includes (child objects) to be returned.</param>
        /// <returns>Entity with the child objects</returns>
        /// <exception cref="ShareBook.Domain.Exceptions.ShareBookException">In case that more than 1 entity could be returned for the filter specified.</exception>
        Task<TEntity> FindAsync(IncludeList<TEntity> includes, Expression<Func<TEntity, bool>> filter);

        /// <summary>
        /// Get ALL the entities, without filter, on the specified order, without child objects.
        /// <para>Use it at your own risk, as the number of data returned could be big.</para>
        /// <para>BE CAREFUL: could impact on the perfomance.</para>
        /// </summary>
        /// <returns>PageList with only 1 page and all the items</returns>
        PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, TKey>> order);

        /// <summary>
        /// Get ALL the entities, without filter, on the specified order, with the specified child objects.
        /// <para>Use it at your own risk, as the number of data returned could be big, and the child is loaded too.</para>
        /// <para>BE CAREFUL: could impact on the perfomance.</para>
        /// </summary>
        /// <returns>PageList with only 1 page and all the items</returns>
        PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, TKey>> order, IncludeList<TEntity> includes);

        /// <summary>
        /// Get ALL the entities based on the filter passed, on the specified order, without child objects.
        /// </summary>
        /// <returns>PageList with only 1 page and all the items</returns>
        PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> order);

        /// <summary>
        /// Get ALL the entities based on the filter passed, on the specified order, with the specified child objects.
        /// </summary>
        /// <returns>PageList with only 1 page and all the items</returns>
        PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> order, IncludeList<TEntity> includes);

        /// <summary>
        /// Get a paged list of the entity, without any filter, on the specified order, without child objects.
        /// </summary>
        /// <param name="page">First Page = 1</param>
        PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, TKey>> order, int page, int itemsPerPage);

        /// <summary>
        /// Get a paged list of the entity, without any filter, on the specified order, with the specified child objects.
        /// </summary>
        /// <param name="page">First Page = 1</param>
        PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, TKey>> order, int page, int itemsPerPage, IncludeList<TEntity> includes);

        /// <summary>
        /// Get a paged list of the entity, based on the filter passed, on the specified order, without child objects.
        /// </summary>
        /// <param name="page">First Page = 1</param>
        PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> order, int page, int itemsPerPage);

        /// <summary>
        /// Get a paged list of the entity, based on the filter passed, on the specified order, with the specified child objects.
        /// </summary>
        /// <param name="page">First Page = 1</param>
        PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> order, int page, int itemsPerPage, IncludeList<TEntity> includes);

        Task<Result<TEntity>> InsertAsync(TEntity entity);
        Task<Result<TEntity>> UpdateAsync(TEntity entity);
        Task<Result> DeleteAsync(params object[] keyValues);
    }
}
