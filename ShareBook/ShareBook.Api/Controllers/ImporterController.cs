using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Api.Filters;
using ShareBook.Api.ViewModels;
using ShareBook.Service.Authorization;
using ShareBook.Service.Importer;
using ShareBook.Service.Upload;
using System.Threading;
using System.Threading.Tasks;

namespace ShareBook.Api.Controllers;

[Route("api/[controller]")]
[EnableCors("AllowAllHeaders")]
public class ImporterController(
    IImporterDashboardService importerDashboardService,
    IUploadService uploadService) : Controller
{
    private readonly IImporterDashboardService _importerDashboardService = importerDashboardService;
    private readonly IUploadService _uploadService = uploadService;

    [HttpGet("ImporterDashboard")]
    [Authorize("Bearer")]
    [AuthorizationFilter(Permissions.Permission.ApproveBook)] // adm
    public async Task<IActionResult> ImporterDashboardAsync(CancellationToken cancellationToken)
    {
        var dashboard = await _importerDashboardService.GetDashboardAsync(cancellationToken);
        return Ok(dashboard);
    }

    [HttpPost("BookThumbnails/Backfill")]
    [Authorize("Bearer")]
    [AuthorizationFilter(Permissions.Permission.ApproveBook)] // adm
    public async Task<IActionResult> BackfillBookThumbnailsAsync(
        [FromQuery] bool overwrite = false,
        [FromQuery] int offset = 0,
        [FromQuery] int batchSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _uploadService.BackfillBookThumbnailsAsync(
            overwrite,
            offset,
            batchSize,
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("ImporterEditorialPrompt")]
    [Authorize("Bearer")]
    [AuthorizationFilter(Permissions.Permission.ApproveBook)] // adm
    public async Task<IActionResult> GetImporterEditorialPromptAsync([FromQuery] string sourceName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(sourceName))
            return BadRequest("sourceName é obrigatório.");

        var prompt = await _importerDashboardService.GetEditorialPromptAsync(sourceName, cancellationToken);
        return Ok(new { sourceName, prompt });
    }

    [HttpPut("ImporterEditorialPrompt")]
    [Authorize("Bearer")]
    [AuthorizationFilter(Permissions.Permission.ApproveBook)] // adm
    public async Task<IActionResult> UpdateImporterEditorialPromptAsync([FromBody] UpdateEditorialPromptVM vm, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(vm?.SourceName))
            return BadRequest("sourceName é obrigatório.");

        await _importerDashboardService.UpdateEditorialPromptAsync(vm.SourceName, vm.Prompt, cancellationToken);
        return Ok();
    }

    [HttpGet("ImporterTranslationPrompt")]
    [Authorize("Bearer")]
    [AuthorizationFilter(Permissions.Permission.ApproveBook)] // adm
    public async Task<IActionResult> GetImporterTranslationPromptAsync([FromQuery] string sourceName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(sourceName))
            return BadRequest("sourceName é obrigatório.");

        var prompt = await _importerDashboardService.GetTranslationPromptAsync(sourceName, cancellationToken);
        return Ok(new { sourceName, prompt });
    }

    [HttpPut("ImporterTranslationPrompt")]
    [Authorize("Bearer")]
    [AuthorizationFilter(Permissions.Permission.ApproveBook)] // adm
    public async Task<IActionResult> UpdateImporterTranslationPromptAsync([FromBody] UpdateEditorialPromptVM vm, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(vm?.SourceName))
            return BadRequest("sourceName é obrigatório.");

        await _importerDashboardService.UpdateTranslationPromptAsync(vm.SourceName, vm.Prompt, cancellationToken);
        return Ok();
    }

    [HttpPatch("ImporterItems/{id}/AdminNotes")]
    [Authorize("Bearer")]
    [AuthorizationFilter(Permissions.Permission.ApproveBook)] // adm
    public async Task<IActionResult> UpdateImporterItemAdminNotesAsync(int id, [FromBody] UpdateImporterItemNotesVM vm, CancellationToken cancellationToken)
    {
        await _importerDashboardService.UpdateAdminNotesAsync(id, vm?.Notes, cancellationToken);
        return Ok();
    }

    [HttpGet("ImporterItems/{id}/History")]
    [Authorize("Bearer")]
    [AuthorizationFilter(Permissions.Permission.ApproveBook)] // adm
    public async Task<IActionResult> GetImporterItemHistoryAsync(int id, CancellationToken cancellationToken)
    {
        var history = await _importerDashboardService.GetItemHistoryAsync(id, cancellationToken);
        return Ok(history);
    }

    [HttpGet("ImporterItems")]
    [Authorize("Bearer")]
    [AuthorizationFilter(Permissions.Permission.ApproveBook)] // adm
    public async Task<IActionResult> ImporterItemsAsync([FromQuery] int? sourceId, [FromQuery] string status, [FromQuery] int? id, [FromQuery] string title, [FromQuery] string sort, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var items = await _importerDashboardService.GetItemsAsync(sourceId, status, id, title, sort, page, pageSize, cancellationToken);
        return Ok(items);
    }
}
