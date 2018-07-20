using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Api.Filters;
using ShareBook.Domain.Common;
using ShareBook.Service.Generic;
using System;
using System.Linq.Expressions;

namespace ShareBook.Api.Controllers
{
    public class BaseController<T> : BaseController<T, T> where T : BaseEntity
    {
        public BaseController(IBaseService<T> service) : base(service) { }
    }

    [GetClaimsFilter]
    [EnableCors("AllowAllHeaders")]
    public class BaseController<T, VM> : Controller
        where T : BaseEntity
        where VM : class
    {
        protected readonly IBaseService<T> _service;
        private Expression<Func<T, object>> _defaultOrder = x => x.Id;
        private bool HasViewModel { get { return typeof(VM) != typeof(T); } }

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
        public Result<VM> Create([FromBody] VM viewModel)
        {
            if (!HasViewModel)
                return Mapper.Map<Result<VM>>(_service.Insert(viewModel as T));
            
            var entity = Mapper.Map<T>(viewModel);
            var result = _service.Insert(entity);
            var resultVM = Mapper.Map<Result<VM>>(result);
            return resultVM;
        }

        [Authorize("Bearer")]
        [HttpPut]
        public Result<VM> Update([FromBody] VM viewModel)
        {
            if (!HasViewModel)
                return Mapper.Map<Result<VM>>(_service.Update(viewModel as T));

            var entity = Mapper.Map<T>(viewModel);
            var result = _service.Update(entity);
            var resultVM = Mapper.Map<VM>(result);
            return new Result<VM>(resultVM);
        }

        [Authorize("Bearer")]
        [HttpDelete("{id}")]
        public Result Delete(string id) => _service.Delete(new Guid(id));
    }
}
