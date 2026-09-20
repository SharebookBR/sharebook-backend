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

namespace ShareBook.Service.Generic;

public class BaseService<TEntity>(ApplicationDbContext context, IUnitOfWork unitOfWork, IValidator<TEntity> validator) : IBaseService<TEntity> where TEntity : class
{
    protected readonly EntityCrud<TEntity> _repository = new EntityCrud<TEntity>(context);
    protected readonly IUnitOfWork _unitOfWork = unitOfWork;
    protected readonly IValidator<TEntity> _validator = validator;

    #region GET

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter) => await _repository.AnyAsync(filter);

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>> filter) => await _repository.CountAsync(filter);

    public virtual async Task<TEntity> FindAsync(object keyValue)
        => await _repository.FindAsync(keyValue);

    public virtual async Task<TEntity> FindAsync(IncludeList<TEntity> includes, object keyValue)
        => await _repository.FindAsync(includes, keyValue);

    public async Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> filter)
        => await _repository.FindAsync(filter);

    public async Task<TEntity> FindAsync(IncludeList<TEntity> includes, Expression<Func<TEntity, bool>> filter) => await _repository.FindAsync(includes, filter);

    public async Task<PagedList<TEntity>> GetAsync<TKey>(Expression<Func<TEntity, TKey>> order, int page, int itemsPerPage, IncludeList<TEntity> includes, bool descending = false)
        => await GetAsync(x => true, order, page, itemsPerPage, includes, descending);

    public virtual async Task<PagedList<TEntity>> GetAsync<TKey>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TKey>> order, int page, int itemsPerPage, bool descending = false)
        => await GetAsync(filter, order, page, itemsPerPage, null, descending);

    /// <summary>
    /// Implementação padrão de paginação. Serviços com uma regra de ordenação/inclusão diferente da
    /// genérica (ex: <see cref="MeetupService"/>) sobrescrevem este método.
    /// </summary>
    public virtual async Task<PagedList<TEntity>> GetAsync<TKey>(
        Expression<Func<TEntity, bool>> filter,
        Expression<Func<TEntity, TKey>> order,
        int page,
        int itemsPerPage,
        IncludeList<TEntity> includes,
        bool descending = false)
        => await _repository.GetAsync(filter, order, page, itemsPerPage, includes, descending);

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

    protected Result<TEntity> Validate(TEntity entity, params Expression<Func<TEntity, object>>[] propertiesToValidate)
    {
        var propertyNames = propertiesToValidate
            .Select(GetPropertyName)
            .ToArray();

        var result = _validator.Validate(entity, options => options.IncludeProperties(propertyNames));
        return new Result<TEntity>(result);
    }

    private static string GetPropertyName(Expression<Func<TEntity, object>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
            return memberExpression.Member.Name;

        if (expression.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression operand)
            return operand.Member.Name;

        throw new ArgumentException("Expression must be a member expression");
    }

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

    public virtual async Task<Result> DeleteAsync(params object[] keyValues)
    {
        await _repository.DeleteAsync(keyValues);
        return new Result();
    }
}
