using System.Threading;
using System.Threading.Tasks;

namespace ShareBook.Service.Analytics;

public interface IAnalyticsService
{
    Task<AnalyticsDashboardDto> GetDashboardAsync();
    Task WarmCacheAsync(string source, CancellationToken cancellationToken = default);
}
