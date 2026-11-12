using Microsoft.AspNetCore.Mvc;
using ShareBook.Service.Onboarding;
using System.Threading;
using System.Threading.Tasks;

namespace ShareBook.Api.Controllers;

/// <summary>Endpoints públicos, mínimos e sem dados pessoais para telemetria do onboarding.</summary>
[Route("o")]
public class OnboardingTrackingController(IOnboardingEmailTrackingService trackingService) : ControllerBase
{
    private static readonly byte[] TransparentGif =
        [71, 73, 70, 56, 57, 97, 1, 0, 1, 0, 128, 0, 0, 0, 0, 0, 255, 255, 255, 33, 249, 4, 1, 0, 0, 0, 0, 44, 0, 0, 0, 0, 1, 0, 1, 0, 0, 2, 2, 76, 1, 0, 59];

    private readonly IOnboardingEmailTrackingService _trackingService = trackingService;

    [HttpGet("open/{token}")]
    public async Task<IActionResult> Open(string token, CancellationToken cancellationToken)
    {
        await _trackingService.RecordOpenAsync(token, cancellationToken);
        Response.Headers.CacheControl = "no-store, no-cache, max-age=0";
        Response.Headers.Pragma = "no-cache";
        return File(TransparentGif, "image/gif");
    }

    [HttpGet("click/{token}/{cta}")]
    public async Task<IActionResult> Click(string token, string cta, CancellationToken cancellationToken)
    {
        var destination = await _trackingService.RecordClickAndGetDestinationAsync(token, cta, cancellationToken);
        return destination == null ? NotFound() : Redirect(destination);
    }
}
