using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sharebook.Jobs;
using ShareBook.Api.Controllers.Generic;
using ShareBook.Api.Filters;
using ShareBook.Service.Server;

namespace ShareBook.Api.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowAllHeaders")]
    public class JobController: BaseJobController
    {

        public JobController(IJobExecutor executor, IOptions<ServerSettings> settings)
        {
            _executor = executor;
            _validToken = settings.Value.JobExecutorToken;
        }

        [HttpGet("Executor")]
        [Throttle(Name = "JobExecutor", Seconds = 300, VaryByIp = false)]
        public IActionResult Executor(){

            if (!_IsValidJobToken()) return Unauthorized();

            var result = _executor.Execute();
            return Ok(result);
        }

    }
}
