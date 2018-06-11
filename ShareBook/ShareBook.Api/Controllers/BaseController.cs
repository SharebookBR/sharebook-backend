using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Api.Filters;
using ShareBook.Domain.Common;
using ShareBook.Service.Generic;
using System;
using System.Linq.Expressions;

namespace ShareBook.Api.Controllers
{
    [GetClaimsFilter]
    public class BaseController<T> : Controller where T : BaseEntity
    {
        protected readonly IBaseService<T> _service;
        private Expression<Func<T, object>> _defaultOrder = x => x.Id;

        public BaseController(IBaseService<T> service)
        {
            _service = service;
        }
        protected void SetDefault(Expression<Func<T, object>> defaultOrder)
        {
            _defaultOrder = defaultOrder;
        }

        [HttpGet()]
        public PagedList<T> GetAll() => GetPaged(1, 15);

        [HttpGet("{page}/{items}")]
        public PagedList<T> GetPaged(int page, int items) => _service.Get(x => true, _defaultOrder, page, items);

        [HttpGet("{id}")]
        public T GetById(string id) => _service.Get(new Guid(id));

        [Authorize("Bearer")]
        [HttpPost]
        public virtual Result<T> Create([FromBody]T entity) => _service.Insert(entity);

        [Authorize("Bearer")]
        [HttpPut]
        public Result<T> Update([FromBody]T entity) => _service.Update(entity);

        [Authorize("Bearer")]
        [HttpDelete("{id}")]
        public Result Delete(string id) => _service.Delete(new Guid(id));
    }
}
