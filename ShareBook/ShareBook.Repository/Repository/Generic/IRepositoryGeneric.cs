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
        /// Execute a Find on the DbSet using the <paramref name="keyValues"/>.
        /// <para>Using this method will return onlye the Entity, without the children.</para>
        /// <para>
        /// If you want to get the Child Objects too, use the <seealso
        /// cref="Find(IncludeList{TEntity}, object[])"/> method.
        /// </para>
        /// </summary>
        TEntity Find(object keyValue);

        /// <summary>
        /// Execute a Find on the DbSet using the <paramref name="keyValues"/> and the <paramref name="includes"/>
        /// </summary>
        TEntity Find(IncludeList<TEntity> includes, object keyValue);

        /// <summary>
        /// Find in the DbSet an entity that matches the specified filter.
        /// </summary>
        /// <returns>Entity with the child objects</returns>
        /// <exception cref="ShareBook.Domain.Exceptions.ShareBookException">
        /// In case that more than 1 entity could be returned for the filter specified.
        /// </exception>
        TEntity Find(Expression<Func<TEntity, bool>> filter);

        /// <summary>
        /// Find in the DbSet an entity that matches the specified filter.
        /// </summary>
        /// <param name="includes">Includes (child objects) to be returned.</param>
        /// <returns>Entity with the child objects</returns>
        /// <exception cref="ShareBook.Domain.Exceptions.ShareBookException">
        /// In case that more than 1 entity could be returned for the filter specified.
        /// </exception>
        TEntity Find(IncludeList<TEntity> includes, Expression<Func<TEntity, bool>> filter);

        /// <summary>
        /// Get ALL the entities, without filter, on the specified order, without child objects.
        /// <para>Use it at your own risk, as the number of data returned could be big.</para>
        /// <para>BE CAREFUL: could impact on the perfomance.</para>
        /// </summary>
        /// <returns>PageList with only 1 page and all the items</returns>
        PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, TKey>> order);

        /// <summary>
        /// Get ALL the entities, without filter, on the specified order, with the specified child objects.
        /// <para>
        /// Use it at your own risk, as the number of data returned could be big, and the child is
        /// loaded too.
        /// </para>
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
        /// Get ALL the entities based on the filter passed, on the specified order, with the
        /// specified child objects.
        /// </summary>
        /// <returns>PageList with only 1 page and all the items</returns>
        PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> order, IncludeList<TEntity> includes);

        /// <summary>
        /// Get a paged list of the entity, without any filter, on the specified order, without
        /// child objects.
        /// </summary>
        /// <param name="page">First Page = 1</param>
        PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, TKey>> order, int page, int itemsPerPage);

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

        /// <summary>
        /// Execute the count based on the specified filter.
        /// <para>
        /// Don't use it to verify if there is any entity that satisfy the condition. To do it, use
        /// the <see cref="Any(Expression{Func{TEntity, bool}})"/> method.
        /// </para>
        /// </summary>
        /// <returns>The number of entities that satisfy the filter</returns>
        int Count(Expression<Func<TEntity, bool>> filter);

        /// <summary>
        /// Verify if there is any entity that satisfy the specified filter.
        /// </summary>
        /// <returns>True in case if there is at least one entity that satisfy the filter.</returns>
        bool Any(Expression<Func<TEntity, bool>> filter);

        TEntity Insert(TEntity entity);

        TEntity Update(TEntity entity);

        void Delete(params object[] keyValues);

        void Delete(TEntity entity);

        IQueryable<TEntity> FromSql(string query, object[] parameters);
    }
}