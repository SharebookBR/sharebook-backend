using Microsoft.EntityFrameworkCore;
using ShareBook.Domain.Common;
using ShareBook.Domain.Exceptions;
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
        public async Task<TEntity> GetAsync(params object[] keyValues) => await _dbSet.FindAsync(keyValues);
        public virtual async Task<PagedList<TEntity>> GetAsync<TKey>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> order, int page, int itemsPerPage)
        {
            var skip = (page - 1) * itemsPerPage;
            var query = _dbSet.Where(filter);
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
            var entity = await GetAsync(keyValues);
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
        public TEntity Get(params object[] keyValues) => _dbSet.Find(keyValues);
        public IQueryable<TEntity> Get() => _dbSet;
        public PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> order, int page, int itemsPerPage) => GetAsync(filter, order, page, itemsPerPage).Result;
        public int Count(Expression<Func<TEntity, bool>> filter) => CountAsync(filter).Result;
        public bool Any(Expression<Func<TEntity, bool>> filter) => AnyAsync(filter).Result;
        public TEntity Insert(TEntity entity) => InsertAsync(entity).Result;
        public TEntity Update(TEntity entity) => UpdateAsync(entity).Result;
        public void Delete(params object[] keyValues) => DeleteAsync(keyValues).Wait();
        public void Delete(TEntity entity) => DeleteAsync(entity).Wait();
        #endregion
    }
}
