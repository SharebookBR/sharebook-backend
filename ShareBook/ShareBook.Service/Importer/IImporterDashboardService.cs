using System.Threading;
using System.Threading.Tasks;
using ShareBook.Domain.DTOs;

namespace ShareBook.Service.Importer;

public interface IImporterDashboardService
{
    Task<ImporterDashboardDTO> GetDashboardAsync(CancellationToken cancellationToken = default);
}
