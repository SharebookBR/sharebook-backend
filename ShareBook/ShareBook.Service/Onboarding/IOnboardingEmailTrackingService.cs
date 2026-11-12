using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShareBook.Service.Onboarding;

public interface IOnboardingEmailTrackingService
{
    string BuildOpenUrl(Guid deliveryId);
    string BuildClickUrl(Guid deliveryId, string cta);
    Task RecordOpenAsync(string token, CancellationToken cancellationToken = default);
    Task<string?> RecordClickAndGetDestinationAsync(string token, string cta, CancellationToken cancellationToken = default);
}
