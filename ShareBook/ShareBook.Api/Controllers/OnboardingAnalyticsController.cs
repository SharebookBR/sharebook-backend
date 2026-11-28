using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Api.Filters;
using ShareBook.Service.Authorization;
using ShareBook.Service.Onboarding;
using System.Threading;
using System.Threading.Tasks;

namespace ShareBook.Api.Controllers;

[Route("api/[controller]")]
public class OnboardingAnalyticsController(
    IOnboardingAnalyticsService onboardingAnalyticsService,
    ILegacyOnboardingRolloutService legacyOnboardingRolloutService) : ControllerBase
{
    private readonly IOnboardingAnalyticsService _onboardingAnalyticsService = onboardingAnalyticsService;
    private readonly ILegacyOnboardingRolloutService _legacyOnboardingRolloutService = legacyOnboardingRolloutService;

    [HttpGet("dashboard")]
    [Authorize("Bearer")]
    [AuthorizationFilter(Permissions.Permission.ApproveBook)]
    public async Task<IActionResult> GetDashboard([FromQuery] int days = 30, CancellationToken cancellationToken = default)
    {
        var dashboard = await _onboardingAnalyticsService.GetDashboardAsync(days, cancellationToken);
        return Ok(dashboard);
    }

    [HttpGet("legacy-rollout")]
    [Authorize("Bearer")]
    [AuthorizationFilter(Permissions.Permission.ApproveBook)]
    public async Task<IActionResult> GetLegacyRollout(CancellationToken cancellationToken = default)
    {
        var dashboard = await _legacyOnboardingRolloutService.GetDashboardAsync(cancellationToken);
        return Ok(dashboard);
    }
}
