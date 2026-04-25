using System.Threading;
using System.Threading.Tasks;
using ShareBook.Domain.DTOs;

namespace ShareBook.Service.Importer;

public interface IImporterDashboardService
{
    Task<ImporterDashboardDTO> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<ImporterQueueItemsPageDTO> GetItemsAsync(int? sourceId, string status, int? position, int page, int pageSize, CancellationToken cancellationToken = default);
}
