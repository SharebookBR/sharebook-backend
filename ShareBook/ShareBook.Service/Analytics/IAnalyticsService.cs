using System.Threading.Tasks;

namespace ShareBook.Service.Analytics;

public interface IAnalyticsService
{
    Task<AnalyticsDashboardDto> GetDashboardAsync();
}
