using System.Threading;
using System.Threading.Tasks;

namespace ShareBook.Service.Onboarding;

public interface ILegacyOnboardingRolloutService
{
    Task<LegacyOnboardingRolloutDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);
}
