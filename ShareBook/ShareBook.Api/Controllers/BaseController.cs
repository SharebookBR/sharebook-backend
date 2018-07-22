using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Api.Filters;
using ShareBook.Api.ViewModels;
using ShareBook.Domain.Common;
using ShareBook.Service.Generic;
using System;
using System.Linq.Expressions;

namespace ShareBook.Api.Controllers
{
    public class BaseController<T> : BaseController<T, T, T>
        where T : BaseEntity
    {
        public BaseController(IBaseService<T> service) : base(service) { }
    }

    public class BaseController<T, R> : BaseController<T, R, T>
        where T : BaseEntity
        where R : BaseViewModel
    {
        public BaseController(IBaseService<T> service) : base(service) { }
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
        private bool HasRequestViewModel { get { return typeof(R) != typeof(T); } }

        public BaseController(IBaseService<T> service)
        {
            _service = service;
        }
        protected void SetDefault(Expression<Func<T, object>> defaultOrder)
        {
            _defaultOrder = defaultOrder;
        }

        [HttpGet()]
        public virtual PagedList<T> GetAll() => Paged(1, 15);

        [HttpGet("{page}/{items}")]
        public virtual PagedList<T> Paged(int page, int items) => _service.Get(x => true, _defaultOrder, page, items);

        [HttpGet("{id}")]
        public T GetById(string id) => _service.Get(new Guid(id));

        [Authorize("Bearer")]
        [HttpPost]
        public virtual Result<A> Create([FromBody] R viewModel)
        {
            if (!HasRequestViewModel)
                return Mapper.Map<Result<A>>(_service.Insert(viewModel as T));

            var entity = Mapper.Map<T>(viewModel);
            var result = _service.Insert(entity);
            var resultVM = Mapper.Map<Result<A>>(result);
            return resultVM;
        }

        [Authorize("Bearer")]
        [HttpPut("{id}")]
        public virtual Result<A> Update(Guid id, [FromBody] R viewModel)
        {
            viewModel.Id = id;

            if (!HasRequestViewModel)
                return Mapper.Map<Result<A>>(_service.Update(viewModel as T));

            var entity = Mapper.Map<T>(viewModel);
            var result = _service.Update(entity);
            var resultVM = Mapper.Map<A>(result);
            return new Result<A>(resultVM);
        }

        [Authorize("Bearer")]
        [HttpDelete("{id}")]
        public Result Delete(Guid id) => _service.Delete(id);
    }
}
