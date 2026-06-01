using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Api.Filters;
using ShareBook.Service.Analytics;
using ShareBook.Service.Authorization;

namespace ShareBook.Api.Controllers;

[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("dashboard")]
    [Authorize("Bearer")]
    [AuthorizationFilter(Permissions.Permission.ApproveBook)]
    public async Task<IActionResult> GetDashboard()
    {
        var dashboard = await _analyticsService.GetDashboardAsync();
        return Ok(dashboard);
    }
}
