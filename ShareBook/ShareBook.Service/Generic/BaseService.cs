using System;
using System.Linq.Expressions;
using FluentValidation;
using ShareBook.Domain.Common;
using ShareBook.Repository;
using ShareBook.Repository.Infra;

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

        public bool Any(Expression<Func<TEntity, bool>> filter) => _repository.Any(filter);
        public int Count(Expression<Func<TEntity, bool>> filter) => _repository.Count(filter);
        public TEntity Get(params object[] keyValues) => _repository.Get(keyValues);
        public PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> order, int page, int itemsPerPage) => _repository.Get(filter, order, page, itemsPerPage);
        protected Result<TEntity> Validate(TEntity entity) => new Result<TEntity>(_validator.Validate(entity));
        protected Result<TEntity> Validate(TEntity entity, params Expression<Func<TEntity, object>>[] filter)  => new Result<TEntity>(_validator.Validate(entity, filter));


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
