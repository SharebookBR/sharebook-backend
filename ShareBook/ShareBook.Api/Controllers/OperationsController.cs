using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sharebook.Jobs;
using ShareBook.Api.Filters;
using ShareBook.Api.ViewModels;
using ShareBook.Helper;
using ShareBook.Service;
using ShareBook.Service.Authorization;
using ShareBook.Service.Server;
using System;
using System.Reflection;

namespace ShareBook.Api.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowAllHeaders")]
    public class OperationsController : Controller
    {

        protected IJobExecutor _executor;
        protected string _validToken;
        IEmailService _emailService;
        private readonly IWebHostEnvironment _env;

        public OperationsController(IJobExecutor executor, IOptions<ServerSettings> settings, IEmailService emailService, IWebHostEnvironment env)
        {
            _executor = executor;
            _validToken = settings.Value.JobExecutorToken;
            _emailService = emailService;
            _env = env;
        }

        [HttpGet]
        [Authorize("Bearer")]
        [AuthorizationFilter(Permissions.Permission.ApproveBook)] // adm
        [Route("ForceException")]
        public IActionResult ForceException()
        {
            var teste = 1 / Convert.ToInt32("Teste");
            return BadRequest();
        }

        [HttpGet("Ping")]
        public IActionResult Ping()
        {
            var result = new
            {
                Service = Assembly.GetEntryAssembly().GetName().Name.ToString(),
                Version = Assembly.GetEntryAssembly().GetName().Version.ToString(),
                DotNetVersion = System.Environment.Version.ToString(),
                Env = _env.EnvironmentName,
            };
            return Ok(result);
        }

        [HttpGet("JobExecutor")]
        [Throttle(Name = "JobExecutor", Seconds = 5, VaryByIp = false)]
        public IActionResult Executor()
        {
            if (!_IsValidJobToken())
                return Unauthorized();
            else
                return Ok(_executor.Execute());
        }

        [HttpPost("EmailTest")]
        [Authorize("Bearer")]
        [AuthorizationFilter(Permissions.Permission.ApproveBook)] // adm
        public IActionResult EmailTest([FromBody] EmailTestVM emailVM)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _emailService.Test(emailVM.Email, emailVM.Name).Wait();
            return Ok();
        }

        protected bool _IsValidJobToken() => Request.Headers["Authorization"].ToString() == _validToken;
    }
}
