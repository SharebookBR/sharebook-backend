using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Api.Filters;
using ShareBook.Api.ViewModels;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Repository.Repository;
using ShareBook.Service;
using ShareBook.Service.Authorization;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Sharebook.Jobs;

namespace ShareBook.Api.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowAllHeaders")]
    public class JobController: Controller
    {
        IJobExecutor _executor;

        public JobController(IJobExecutor executor)
        {
            _executor = executor;
        }

        [HttpGet("Execute")]
        public IActionResult Ping(){
            var result = _executor.Execute();
            return Ok(result);
        }
    }
}
