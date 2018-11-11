using FluentValidation;
using ShareBook.Domain.Common;
using ShareBook.Repository;
using ShareBook.Repository.Repository;
using ShareBook.Repository.UoW;
using System;
using System.Linq.Expressions;

namespace ShareBook.Service.Generic
{
    public class BaseService<TEntity> : IBaseService<TEntity> where TEntity : class
    {
        protected readonly IRepositoryGeneric<TEntity> _repository;
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IValidator<TEntity> _validator;

        public BaseService(IRepositoryGeneric<TEntity> repository, IUnitOfWork unitOfWork, IValidator<TEntity> validator)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _validator = validator;
        }

        #region GET
        public bool Any(Expression<Func<TEntity, bool>> filter) => _repository.Any(filter);
        public int Count(Expression<Func<TEntity, bool>> filter) => _repository.Count(filter);

        public virtual TEntity Find(object keyValue)
            => _repository.Find(keyValue);
        public virtual TEntity Find(IncludeList<TEntity> includes, object keyValue)
            => _repository.Find(includes, keyValue);
        public TEntity Find(Expression<Func<TEntity, bool>> filter)
            => _repository.Find(filter);
        public TEntity Find(IncludeList<TEntity> includes, Expression<Func<TEntity, bool>> filter) => _repository.Find(includes, filter);

        public PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, TKey>> order)
            => _repository.Get(order);

        public PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, TKey>> order, IncludeList<TEntity> includes)
            => _repository.Get(order, includes);

        public PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> order)
            => _repository.Get(filter, order);

        public PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> order, IncludeList<TEntity> includes)
            => _repository.Get(filter, order, includes);

        public PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, TKey>> order, int page, int itemsPerPage)
            => _repository.Get(order, page, itemsPerPage);

        public PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, TKey>> order, int page, int itemsPerPage, IncludeList<TEntity> includes)
            => _repository.Get(order, page, itemsPerPage, includes);

        public virtual PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> order, int page, int itemsPerPage)
            => _repository.Get(filter, order, page, itemsPerPage);

        public PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> order, int page, int itemsPerPage, IncludeList<TEntity> includes)
            => _repository.Get(filter, order, page, itemsPerPage, includes);
        #endregion

        protected Result<TEntity> Validate(TEntity entity) => new Result<TEntity>(_validator.Validate(entity));
        protected Result<TEntity> Validate(TEntity entity, params Expression<Func<TEntity, object>>[] filter) => new Result<TEntity>(_validator.Validate(entity, filter));

        public virtual Result<TEntity> Insert(TEntity entity)
        {
            var result = Validate(entity);

            if (result.Success)
                result.Value = _repository.Insert(entity);

            return result;
        }
        public virtual Result<TEntity> Update(TEntity entity)
        {
            var result = Validate(entity);

            if (result.Success)
                result.Value = _repository.Update(entity);

            return result;
        }
        public Result Delete(params object[] keyValues)
        {
            _repository.Delete(keyValues);
            return new Result();
        }
    }
}
