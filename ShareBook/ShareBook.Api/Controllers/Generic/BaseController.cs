using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Api.Filters;
using ShareBook.Api.ViewModels;
using ShareBook.Domain.Common;
using ShareBook.Service.Generic;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ShareBook.Api.Controllers;

public class BaseController<T>(IBaseService<T> service) : BaseController<T, T, T>(service)
    where T : BaseEntity
{
}

public class BaseController<T, R>(IBaseService<T> service) : BaseController<T, R, T>(service)
    where T : BaseEntity
    where R : BaseViewModel
{
}

[GetClaimsFilter]
[EnableCors("AllowAllHeaders")]
public class BaseController<T, R, A>(IBaseService<T> service) : Controller
    where T : BaseEntity
    where R : IIdProperty
    where A : class
{
    protected readonly IBaseService<T> _service = service;
    private Expression<Func<T, object>> _defaultOrder = x => x.Id;
    protected bool HasRequestViewModel { get { return typeof(R) != typeof(T); } }

    protected void SetDefault(Expression<Func<T, object>> defaultOrder)
    {
        _defaultOrder = defaultOrder;
    }

    [HttpGet()]
    public virtual async Task<PagedList<T>> GetAllAsync() => await PagedAsync(1, 15);

    [HttpGet("{page}/{items}")]
    public virtual async Task<PagedList<T>> PagedAsync(int page, int items) => await _service.GetAsync(x => true, _defaultOrder, page, items);

    [HttpGet("{id}")]
    public virtual async Task<T> GetByIdAsync(string id) => await _service.FindAsync(new Guid(id));
}
