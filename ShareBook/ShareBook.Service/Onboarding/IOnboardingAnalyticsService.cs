using System.Threading;
using System.Threading.Tasks;

namespace ShareBook.Service.Onboarding;

public interface IOnboardingAnalyticsService
{
    Task<OnboardingAnalyticsDashboardDto> GetDashboardAsync(int days, CancellationToken cancellationToken = default);
}
