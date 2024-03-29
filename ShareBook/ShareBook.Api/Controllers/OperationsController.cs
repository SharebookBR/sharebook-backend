﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Sharebook.Jobs;
using ShareBook.Api.Filters;
using ShareBook.Api.ViewModels;
using ShareBook.Helper.Extensions;
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
        private readonly IConfiguration _configuration;
        private IMemoryCache _cache;

        public OperationsController(IJobExecutor executor, IOptions<ServerSettings> settings, IEmailService emailService, IWebHostEnvironment env, IConfiguration configuration, IMemoryCache memoryCache)
        {
            _executor = executor;
            _validToken = settings.Value.JobExecutorToken;
            _emailService = emailService;
            _env = env;
            _configuration = configuration;
            _cache = memoryCache;
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
            var ass = Assembly.GetEntryAssembly();
            var result = new
            {
                Service = ass.GetName().Name.ToString(),
                Version = ass.GetName().Version.ToString(),
                DotNetVersion = System.Environment.Version.ToString(),
                BuildLinkerTime = ass.GetLinkerTime().ToString("dd/MM/yyyy HH:mm:ss:fff z"),
                Env = _env.EnvironmentName,
                TimeZone = TimeZoneInfo.Local.DisplayName,
                System.Runtime.InteropServices.RuntimeInformation.OSDescription,
                MemoryCacheCount = ((MemoryCache)_cache).Count
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
