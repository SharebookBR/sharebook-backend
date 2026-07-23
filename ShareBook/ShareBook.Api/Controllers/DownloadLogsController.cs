using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Api.Filters;
using ShareBook.Service.Authorization;
using ShareBook.Service.DownloadLogs;

namespace ShareBook.Api.Controllers;

[Route("api/[controller]")]
[Authorize("Bearer")]
[AuthorizationFilter(Permissions.Permission.ApproveBook)]
public class DownloadLogsController : ControllerBase
{
    private readonly IDownloadLogsService _downloadLogsService;

    public DownloadLogsController(IDownloadLogsService downloadLogsService)
    {
        _downloadLogsService = downloadLogsService;
    }

    [HttpGet("Summary")]
    public async Task<IActionResult> GetSummaryAsync([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var (rangeFrom, rangeTo) = ResolveRange(from, to);
        var summary = await _downloadLogsService.GetSummaryAsync(rangeFrom, rangeTo);
        return Ok(summary);
    }

    [HttpGet]
    public async Task<IActionResult> GetEventsAsync(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        var (rangeFrom, rangeTo) = ResolveRange(from, to);
        var paged = await _downloadLogsService.GetEventsAsync(rangeFrom, rangeTo, page, pageSize);
        return Ok(paged);
    }

    private static (DateTime from, DateTime to) ResolveRange(DateTime? from, DateTime? to)
    {
        var resolvedTo = (to ?? DateTime.Today).Date;
        var resolvedFrom = (from ?? resolvedTo.AddDays(-6)).Date;
        return (resolvedFrom, resolvedTo);
    }
}
