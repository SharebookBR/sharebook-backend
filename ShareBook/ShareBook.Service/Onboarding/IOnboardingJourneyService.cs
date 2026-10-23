using ShareBook.Domain;

namespace ShareBook.Service.Onboarding;

public interface IOnboardingJourneyService
{
    Task EnrollAsync(User user);
    Task CancelAsync(Guid userId, string reason);
}
