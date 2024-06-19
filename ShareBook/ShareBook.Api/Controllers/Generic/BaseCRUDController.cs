using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Api.Filters;
using ShareBook.Api.ViewModels;
using ShareBook.Domain.Common;
using ShareBook.Service.Generic;
using System;
using System.Threading.Tasks;

namespace ShareBook.Api.Controllers
{
    public class BaseCrudController<T> : BaseCrudController<T, T, T>
        where T : BaseEntity
    {
        public BaseCrudController(IBaseService<T> service, IMapper mapper) : base(service, mapper) { }
    }

    public class BaseCrudController<T, R> : BaseDeleteController<T, R, T>
       where T : BaseEntity
       where R : BaseViewModel
    {
        public BaseCrudController(IBaseService<T> service) : base(service)
        {
        }
    }

    [GetClaimsFilter]
    [EnableCors("AllowAllHeaders")]
    public class BaseCrudController<T, R, A> : BaseDeleteController<T, R, A>
        where T : BaseEntity
        where R : IIdProperty
        where A : class
    {
        protected readonly IMapper _mapper;

        public BaseCrudController(IBaseService<T> service, IMapper mapper) : base(service)
        {
            _mapper = mapper;
        }

        [Authorize("Bearer")]
        [HttpPost]
        public virtual async Task<Result<A>> CreateAsync([FromBody] R viewModel)
        {
            if (!HasRequestViewModel)
                return _mapper.Map<Result<A>>(await _service.InsertAsync(viewModel as T));

            var entity = _mapper.Map<T>(viewModel);
            var result = await _service.InsertAsync(entity);

            var resultVM = _mapper.Map<Result<A>>(result);
            return resultVM;
        }

        [Authorize("Bearer")]
        [HttpPut("{id}")]
        public virtual async Task<Result<A>> UpdateAsync(Guid id, [FromBody] R viewModel)
        {
            viewModel.Id = id;

            if (!HasRequestViewModel)
                return _mapper.Map<Result<A>>(await _service.UpdateAsync(viewModel as T));
            
            var entity = _mapper.Map<T>(viewModel);
            var result = await _service.UpdateAsync(entity);
            var resultVM = _mapper.Map<A>(result);
            return new Result<A>(resultVM);
        }
    }
}