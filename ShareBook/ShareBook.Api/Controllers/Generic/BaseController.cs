using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Api.Filters;
using ShareBook.Api.ViewModels;
using ShareBook.Domain.Common;
using ShareBook.Service.Generic;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ShareBook.Api.Controllers
{
    public class BaseController<T> : BaseController<T, T, T>
        where T : BaseEntity
    {
        public BaseController(IBaseService<T> service) : base(service)
        {
        }
    }

    public class BaseController<T, R> : BaseController<T, R, T>
        where T : BaseEntity
        where R : BaseViewModel
    {
        public BaseController(IBaseService<T> service) : base(service)
        {
        }
    }

    [GetClaimsFilter]
    [EnableCors("AllowAllHeaders")]
    public class BaseController<T, R, A> : Controller
        where T : BaseEntity
        where R : IIdProperty
        where A : class
    {
        protected readonly IBaseService<T> _service;
        private Expression<Func<T, object>> _defaultOrder = x => x.Id;
        protected bool HasRequestViewModel { get { return typeof(R) != typeof(T); } }

        public BaseController(IBaseService<T> service)
        {
            _service = service;
        }

        protected void SetDefault(Expression<Func<T, object>> defaultOrder)
        {
            _defaultOrder = defaultOrder;
        }

        [HttpGet()]
        public virtual async Task<PagedList<T>> GetAllAsync() => await PagedAsync(1, 15);

        [HttpGet("{page}/{items}")]
        public virtual async Task<PagedList<T>> PagedAsync(int page, int items) => await _service.GetAsync(x => true, _defaultOrder, page, items);

        [HttpGet("{id}")]
        public async Task<T> GetByIdAsync(string id) => await _service.FindAsync(new Guid(id));
    }
}