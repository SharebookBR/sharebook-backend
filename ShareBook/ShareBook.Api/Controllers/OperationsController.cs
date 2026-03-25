using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Sharebook.Jobs;
using ShareBook.Api.Filters;
using ShareBook.Api.ViewModels;
using ShareBook.Helper.Extensions;
using ShareBook.Repository;
using ShareBook.Service;
using ShareBook.Service.Authorization;
using ShareBook.Service.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ShareBook.Api.Controllers;

[Route("api/[controller]")]
[EnableCors("AllowAllHeaders")]
public class OperationsController : Controller
{

    protected IJobExecutor _executor;
    protected string _validToken;
    readonly IEmailService _emailService;
    private readonly IWebHostEnvironment _env;
    private readonly IMemoryCache _cache;
    private readonly IMeetupService _meetupService;
    private readonly IConfiguration _config;
    private readonly IJobHistoryRepository _jobHistoryRepo;
    private readonly IList<IJob> _jobs;

    public OperationsController(
        IJobExecutor executor,
        IOptions<ServerSettings> settings,
        IEmailService emailService,
        IWebHostEnvironment env,
        IMemoryCache memoryCache,
        IMeetupService meetupService,
        IConfiguration config,
        IJobHistoryRepository jobHistoryRepo,
        CancelAbandonedDonations job0,
        ChooseDateReminder job1,
        LateDonationNotification job2,
        RemoveBookFromShowcase job3,
        MeetupSearch job4,
        NewBookGetInterestedUsers job5,
        MailSupressListUpdate job6,
        MailSender job7,
        NewEbookWeeklyDigest job8)
    {
        _executor = executor;
        _validToken = settings.Value.JobExecutorToken;
        _emailService = emailService;
        _env = env;
        _cache = memoryCache;
        _meetupService = meetupService;
        _config = config;
        _jobHistoryRepo = jobHistoryRepo;
        _jobs = new List<IJob> { job0, job1, job2, job3, job4, job5, job6, job7, job8 };
    }

    [HttpGet]
    [Authorize("Bearer")]
    [AuthorizationFilter(Permissions.Permission.ApproveBook)] // adm
    [Route("ForceException")]
    public IActionResult ForceException()
    {
        _ = 1 / Convert.ToInt32("Teste");
        return BadRequest();
    }

    [HttpGet("Ping")]
    public IActionResult Ping()
    {
        var ass = Assembly.GetEntryAssembly();
        var result = new
        {
            DatabaseProvider = _config["DatabaseProvider"],
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
    public async Task<IActionResult> ExecutorAsync()
    {
        if (!_IsValidJobToken())
            return Unauthorized();
        else
            return Ok(await _executor.ExecuteAsync());
    }

    [HttpPost("EmailTest")]
    [Authorize("Bearer")]
    [AuthorizationFilter(Permissions.Permission.ApproveBook)] // adm
    public async Task<IActionResult> EmailTestAsync([FromBody] EmailTestVM emailVM)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _emailService.TestAsync(emailVM.Email, emailVM.Name);
        return Ok();
    }

    [HttpPost("JobTest")]
    [Authorize("Bearer")]
    [AuthorizationFilter(Permissions.Permission.ApproveBook)] // adm
    public async Task<IActionResult> JobTestAsync()
    {
        var logs = await _emailService.ProcessBounceMessagesAsync();
        return Ok(logs);
    }

    [HttpGet("Jobs")]
    [Authorize("Bearer")]
    [AuthorizationFilter(Permissions.Permission.ApproveBook)] // adm
    public async Task<IActionResult> JobsAsync()
    {
        var executorHistory = await _jobHistoryRepo
            .Get()
            .AsNoTracking()
            .Where(x => x.JobName == "JobExecutor")
            .OrderByDescending(x => x.CreationDate)
            .FirstOrDefaultAsync();

        var jobs = new List<JobMonitorItemVM>();

        foreach (var job in _jobs)
        {
            var lastHistory = await _jobHistoryRepo
                .Get()
                .AsNoTracking()
                .Where(x => x.JobName == job.JobName)
                .OrderByDescending(x => x.CreationDate)
                .FirstOrDefaultAsync();

            jobs.Add(new JobMonitorItemVM()
            {
                Order = jobs.Count + 1,
                JobName = job.JobName,
                Description = NormalizeSingleLine(job.Description),
                Interval = job.Interval.ToString(),
                Active = job.Active,
                BestDayOfWeek = job.BestDayOfWeek?.ToString(),
                BestTimeToExecute = job.BestTimeToExecute?.ToString(@"hh\:mm"),
                NextExecutionAt = job.GetNextExecutionAtUtc(),
                LastExecutionAt = lastHistory?.CreationDate,
                LastExecutionDurationSeconds = lastHistory?.TimeSpentSeconds,
                LastExecutionDetails = lastHistory?.Details
            });
        }

        var dashboard = new JobMonitorDashboardVM()
        {
            Summary = new JobMonitorSummaryVM()
            {
                TotalJobs = jobs.Count,
                ActiveJobs = jobs.Count(x => x.Active),
                InactiveJobs = jobs.Count(x => !x.Active),
                JobsWithHistory = jobs.Count(x => x.LastExecutionAt.HasValue),
                JobsNeverExecuted = jobs.Count(x => !x.LastExecutionAt.HasValue),
            },
            Executor = new JobMonitorExecutorVM()
            {
                LastExecutionAt = executorHistory?.CreationDate,
                LastExecutionSuccess = executorHistory?.IsSuccess,
                LastExecutionDurationSeconds = executorHistory?.TimeSpentSeconds,
                Details = executorHistory?.Details
            },
            Jobs = jobs
        };

        return Ok(dashboard);
    }

    private static string NormalizeSingleLine(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        return string.Join(" ", value
            .Split(new[] { '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim()));
    }

    protected bool _IsValidJobToken() => Request.Headers["Authorization"].ToString() == _validToken;
}
