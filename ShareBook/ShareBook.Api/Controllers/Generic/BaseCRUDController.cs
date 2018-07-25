using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Api.Filters;
using ShareBook.Api.ViewModels;
using ShareBook.Domain.Common;
using ShareBook.Service.Generic;
using System;

namespace ShareBook.Api.Controllers
{
    public class BaseCRUDController<T> : BaseCRUDController<T, T, T>
        where T : BaseEntity
    {
        public BaseCRUDController(IBaseService<T> service) : base(service) { }
    }

    public class BaseCRUDController<T, R> : BaseDeleteController<T, R, T>
       where T : BaseEntity
       where R : BaseViewModel
    {
        public BaseCRUDController(IBaseService<T> service) : base(service) { }
    }

    [GetClaimsFilter]
    [EnableCors("AllowAllHeaders")]
    public class BaseCRUDController<T, R, A> : BaseDeleteController<T, R, A>
        where T : BaseEntity
        where R : IIdProperty
        where A : class
    {

        public BaseCRUDController(IBaseService<T> service) : base(service) { }

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
    }
}
