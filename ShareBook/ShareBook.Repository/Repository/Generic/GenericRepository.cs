using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ShareBook.Data;

namespace ShareBook.Repository
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<TEntity> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<TEntity>();
        }

        public TEntity FirstOrDefault(Expression<Func<TEntity, bool>> where)
        {
            return _dbSet.FirstOrDefault(where);
        }

        public IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> where)
        {
            return _dbSet.Where(where).ToList();
        }

        public IEnumerable<TEntity> GetAll()
        {
            return _dbSet.ToList();
        }

        public virtual TEntity GetById(params object[] keys)
        {
            return _dbSet.Find(keys);
        }

        public virtual TEntity Insert(TEntity entity)
        {
            _context.Add(entity);
            return entity;
        }

        public TEntity SingleOrDefault(Expression<Func<TEntity, bool>> where)
        {
            return _dbSet.SingleOrDefault(where);
        }

        public virtual TEntity Update(TEntity entity)
        {
            _context.Update(entity);
            return entity;
        }
    }
}