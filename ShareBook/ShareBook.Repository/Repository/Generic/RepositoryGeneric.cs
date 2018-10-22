using Microsoft.EntityFrameworkCore;
using ShareBook.Domain.Common;
using ShareBook.Domain.Exceptions;
using ShareBook.Repository.Repository;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ShareBook.Repository
{
    public class RepositoryGeneric<TEntity> : IRepositoryGeneric<TEntity> where TEntity : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        public RepositoryGeneric(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }

        #region Asynchronous
        public async Task<TEntity> FindAsync(params object[] keyValues) => await FindAsync(null, keyValues);
        public async Task<TEntity> FindAsync(IncludeList<TEntity> includes, params object[] keyValues)
        {
            var result = await _dbSet.FindAsync(keyValues);

            if (includes != null)
                foreach (var item in includes)
                    await _context.Entry(result).Reference(item).LoadAsync();

            return result;
        }
        public async Task<TEntity> FindAsync(IncludeList<TEntity> includes, Expression<Func<TEntity, bool>> filter)
        {
            var query = _dbSet.AsQueryable();

            if (includes != null)
                foreach (var item in includes)
                    query = query.Include(item);

            query = query.Where(filter);

            var count = await query.CountAsync();
            if (count > 1)
                throw new ShareBookException("More than one entity find for the specified filter");

            return query.FirstOrDefault();
        }

        public virtual async Task<PagedList<TEntity>> GetAsync<TKey>(
           Expression<Func<TEntity, bool>> filter,
           Expression<Func<TEntity, TKey>> order,
           int page,
           int itemsPerPage)
        {
            return await GetAsync(filter, order, page, itemsPerPage, null);
        }

        public virtual async Task<PagedList<TEntity>> GetAsync<TKey>(
            Expression<Func<TEntity, bool>> filter,
            Expression<Func<TEntity, TKey>> order,
            int page,
            int itemsPerPage,
            IncludeList<TEntity> includes)
        {
            var skip = (page - 1) * itemsPerPage;
            var query = _dbSet.AsQueryable();

            if (includes != null)
                foreach (var item in includes)
                    query = query.Include(item);

            query = query.Where(filter);
            var total = await query.CountAsync();
            var result = await query
                .OrderBy(order)
                .Skip(skip)
                .Take(itemsPerPage)
                .ToListAsync();

            return new PagedList<TEntity>()
            {
                Page = page,
                ItemsPerPage = itemsPerPage,
                TotalItems = total,
                Items = result
            };
        }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> filter) => await _dbSet.CountAsync(filter);

        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter) => await CountAsync(filter) > 0;

        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            _context.Add(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        public virtual async Task<TEntity> UpdateAsync(TEntity entity)
        {
            _context.Update(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        public async Task DeleteAsync(params object[] keyValues)
        {
            var entity = await FindAsync(keyValues);
            if (entity == null)
                throw new ShareBookException(ShareBookException.Error.NotFound);
            await DeleteAsync(entity);
        }

        public async Task DeleteAsync(TEntity entity)
        {
            _context.Remove(entity);
            await _context.SaveChangesAsync();
        }
        #endregion

        #region Synchronous
        public IQueryable<TEntity> Get() => _dbSet;

        public TEntity Find(object keyValue) => _dbSet.Find(keyValue);

        public TEntity Find(IncludeList<TEntity> includes, object keyValue) => FindAsync(includes, keyValue).Result;

        public TEntity Find(Expression<Func<TEntity, bool>> filter) => FindAsync(null, filter).Result;

        public TEntity Find(IncludeList<TEntity> includes, Expression<Func<TEntity, bool>> filter) => FindAsync(includes, filter).Result;

        public PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, TKey>> order)
            => Get(order, null);

        public PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, TKey>> order, IncludeList<TEntity> includes)
            => Get(x => true, order, includes);

        public PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> order)
            => Get(filter, order, null);

        public PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> order, IncludeList<TEntity> includes)
           => Get(filter, order, 1, int.MaxValue, includes);

        public PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, TKey>> order, int page, int itemsPerPage)
            => Get(order, page, itemsPerPage, null);

        public PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, TKey>> order, int page, int itemsPerPage, IncludeList<TEntity> includes)
            => Get(x => true, order, page, itemsPerPage, includes);

        public PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> order, int page, int itemsPerPage)
            => Get(filter, order, page, itemsPerPage, null);

        public PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> order, int page, int itemsPerPage, IncludeList<TEntity> includes)
            => GetAsync(filter, order, page, itemsPerPage, includes).Result;

        public int Count(Expression<Func<TEntity, bool>> filter) => CountAsync(filter).Result;

        public bool Any(Expression<Func<TEntity, bool>> filter) => AnyAsync(filter).Result;

        public TEntity Insert(TEntity entity) => InsertAsync(entity).Result;

        public TEntity Update(TEntity entity) => UpdateAsync(entity).Result;

        public void Delete(params object[] keyValues) => DeleteAsync(keyValues).Wait();

        public void Delete(TEntity entity) => DeleteAsync(entity).Wait();
        #endregion
    }
}
