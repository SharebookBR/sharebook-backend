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

public class BaseDeleteController<T>(IBaseService<T> service) : BaseDeleteController<T, T, T>(service)
    where T : BaseEntity
{
}

public class BaseDeleteController<T, R>(IBaseService<T> service) : BaseDeleteController<T, R, T>(service)
   where T : BaseEntity
   where R : BaseViewModel
{
}

[GetClaimsFilter]
[EnableCors("AllowAllHeaders")]
public class BaseDeleteController<T, R, A>(IBaseService<T> service) : BaseController<T, R, A>(service)
    where T : BaseEntity
    where R : IIdProperty
    where A : class
{
    [Authorize("Bearer")]
    [HttpDelete("{id}")]
    public async Task<Result> Delete(Guid id) => await _service.DeleteAsync(id);
}
