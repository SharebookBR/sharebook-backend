using ShareBook.Domain.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ShareBook.Service.Importer;

public interface IImporterDashboardService
{
    Task<ImporterDashboardDTO> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<ImporterQueueItemsPageDTO> GetItemsAsync(int? sourceId, string status, int? id, string title, string sort, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<string> GetEditorialPromptAsync(string sourceName, CancellationToken cancellationToken = default);
    Task UpdateEditorialPromptAsync(string sourceName, string prompt, CancellationToken cancellationToken = default);
    Task<string> GetTranslationPromptAsync(string sourceName, CancellationToken cancellationToken = default);
    Task UpdateTranslationPromptAsync(string sourceName, string prompt, CancellationToken cancellationToken = default);
    Task UpdateAdminNotesAsync(int id, string notes, CancellationToken cancellationToken = default);
    Task<IList<ImporterQueueItemHistoryEntryDTO>> GetItemHistoryAsync(int itemId, CancellationToken cancellationToken = default);
}
