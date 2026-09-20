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

namespace ShareBook.Api.Controllers;

public class BaseCrudController<T>(IBaseService<T> service, IMapper mapper) : BaseCrudController<T, T, T>(service, mapper)
    where T : BaseEntity
{
}

public class BaseCrudController<T, R>(IBaseService<T> service) : BaseDeleteController<T, R, T>(service)
   where T : BaseEntity
   where R : BaseViewModel
{
}

[GetClaimsFilter]
[EnableCors("AllowAllHeaders")]
public class BaseCrudController<T, R, A>(IBaseService<T> service, IMapper mapper) : BaseDeleteController<T, R, A>(service)
    where T : BaseEntity
    where R : IIdProperty
    where A : class
{
    protected readonly IMapper _mapper = mapper;

    [Authorize("Bearer")]
    [HttpPost]
    public virtual async Task<Result<A>> CreateAsync([FromBody] R viewModel)
    {
        if (!HasRequestViewModel)
            return _mapper.Map<Result<A>>(await _service.InsertAsync((viewModel as T)!));

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
            return _mapper.Map<Result<A>>(await _service.UpdateAsync((viewModel as T)!));

        var entity = _mapper.Map<T>(viewModel);
        var result = await _service.UpdateAsync(entity);
        var resultVM = new Result<A>(null, result.Value == null ? null : _mapper.Map<A>(result.Value))
        {
            SuccessMessage = result.SuccessMessage
        };
        resultVM.Messages.AddRange(result.Messages);

        return resultVM;
    }
}
