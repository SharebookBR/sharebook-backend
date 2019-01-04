using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sharebook.Jobs;
using ShareBook.Service.Server;
using System.Configuration;
using ShareBook.Api.Filters;

namespace ShareBook.Api.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowAllHeaders")]
    public class JobController: Controller
    {
        IJobExecutor _executor;
        string _validToken;

        public JobController(IJobExecutor executor, IOptions<ServerSettings> settings)
        {
            _executor = executor;
            _validToken = settings.Value.JobExecutorToken;
        }

        [HttpGet("Executor")]
        [Throttle(Name = "JobExecutor", Seconds = 5)]
        // TODO: colocar 300 segundos ( 5 minutos ) 
        public IActionResult Executor(){

            if (!_IsValidJobToken()) return Unauthorized();

            var result = _executor.Execute();
            return Ok(result);
        }

        private bool _IsValidJobToken()
        {
            var userToken = Request.Headers["Authorization"].ToString();

            if (userToken == _validToken) return true;

            return false;
        }
    }
}
