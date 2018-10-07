using ShareBook.Domain.Common;
using System;
using System.Linq.Expressions;

namespace ShareBook.Service.Generic
{
    public interface IBaseService<TEntity> where TEntity : class
    {
        bool Any(Expression<Func<TEntity, bool>> filter);
        int Count(Expression<Func<TEntity, bool>> filter);
        TEntity Find(params object[] keyValues);
        PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> order, int page, int itemsPerPage);
        Result<TEntity> Insert(TEntity entity);
        Result<TEntity> Update(TEntity entity);
        Result Delete(params object[] keyValues);
    }
}
