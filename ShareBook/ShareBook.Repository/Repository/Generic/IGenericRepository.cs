using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ShareBook.Repository
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        TEntity FirstOrDefault(Expression<Func<TEntity, bool>> where);

        IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> where);

        IEnumerable<TEntity> GetAll();

        TEntity GetById(params object[] keys);

        TEntity Insert(TEntity entity);

        TEntity SingleOrDefault(Expression<Func<TEntity, bool>> where);

        TEntity Update(TEntity entity);
    }
}