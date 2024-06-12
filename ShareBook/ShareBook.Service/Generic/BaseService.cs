using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ShareBook.Domain.Common;
using ShareBook.Repository;
using ShareBook.Repository.Repository;
using ShareBook.Repository.UoW;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

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

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> filter) => await _repository.CountAsync(filter);

        public virtual async Task<TEntity> FindAsync(object keyValue)
            => await _repository.FindAsync(keyValue);

        public virtual async Task<TEntity> FindAsync(IncludeList<TEntity> includes, object keyValue)
            => await _repository.FindAsync(includes, keyValue);

        public TEntity Find(Expression<Func<TEntity, bool>> filter)
            => _repository.Find(filter);

        public async Task<TEntity> FindAsync(IncludeList<TEntity> includes, Expression<Func<TEntity, bool>> filter) => await _repository.FindAsync(includes, filter);

        public PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, TKey>> order)
            => _repository.Get(order);

        public PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, TKey>> order, IncludeList<TEntity> includes)
            => _repository.Get(order, includes);

        public PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> order)
            => _repository.Get(filter, order);

        public PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> order, IncludeList<TEntity> includes)
            => _repository.Get(filter, order, includes);

        public PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, TKey>> order, int page, int itemsPerPage, IncludeList<TEntity> includes)
            => _repository.Get(order, page, itemsPerPage, includes);

        public virtual PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> order, int page, int itemsPerPage)
            => _repository.Get(filter, order, page, itemsPerPage);

        public PagedList<TEntity> Get<TKey>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> order, int page, int itemsPerPage, IncludeList<TEntity> includes)
            => _repository.Get(filter, order, page, itemsPerPage, includes);

        public async Task<PagedList<TEntity>> FormatPagedListAsync(IQueryable<TEntity> query, int page, int itemsPerPage)
        {
            var total = await query.CountAsync();
            var skip = (page - 1) * itemsPerPage;
            return new PagedList<TEntity>()
            {
                Page = page,
                ItemsPerPage = itemsPerPage,
                TotalItems = total,
                Items = await query.Skip(skip).Take(itemsPerPage).ToListAsync()
            };
        }

        #endregion GET

        protected async Task<Result<TEntity>> ValidateAsync(TEntity entity) => new Result<TEntity>(await _validator.ValidateAsync(entity));

        protected Result<TEntity> Validate(TEntity entity, params Expression<Func<TEntity, object>>[] filter) => new Result<TEntity>(_validator.Validate(entity, filter));

        public virtual async Task<Result<TEntity>> InsertAsync(TEntity entity)
        {
            var result = await ValidateAsync(entity);

            if (result.Success)
                result.Value = await _repository.InsertAsync(entity);

            return result;
        }

        public virtual async Task<Result<TEntity>> UpdateAsync(TEntity entity)
        {
            var result = await ValidateAsync(entity);

            if (result.Success)
                result.Value = await _repository.UpdateAsync(entity);

            return result;
        }

        public async Task<Result> DeleteAsync(params object[] keyValues)
        {
            await _repository.DeleteAsync(keyValues);
            return new Result();
        }
    }
}