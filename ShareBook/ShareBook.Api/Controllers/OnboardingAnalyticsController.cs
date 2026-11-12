using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Api.Filters;
using ShareBook.Service.Authorization;
using ShareBook.Service.Onboarding;
using System.Threading;
using System.Threading.Tasks;

namespace ShareBook.Api.Controllers;

[Route("api/[controller]")]
public class OnboardingAnalyticsController(IOnboardingAnalyticsService onboardingAnalyticsService) : ControllerBase
{
    private readonly IOnboardingAnalyticsService _onboardingAnalyticsService = onboardingAnalyticsService;

    [HttpGet("dashboard")]
    [Authorize("Bearer")]
    [AuthorizationFilter(Permissions.Permission.ApproveBook)]
    public async Task<IActionResult> GetDashboard([FromQuery] int days = 30, CancellationToken cancellationToken = default)
    {
        var dashboard = await _onboardingAnalyticsService.GetDashboardAsync(days, cancellationToken);
        return Ok(dashboard);
    }
}
