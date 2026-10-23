using Microsoft.EntityFrameworkCore;
using ShareBook.Domain;
using ShareBook.Repository;

namespace ShareBook.Service.Onboarding;

public class OnboardingJourneyService(ApplicationDbContext context, TimeProvider timeProvider) : IOnboardingJourneyService
{
    private readonly ApplicationDbContext _context = context;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task EnrollAsync(User user)
    {
        if (!user.Active || !user.ParentAproved || !user.AllowSendingEmail)
            return;

        var alreadyEnrolled = await _context.OnboardingJourneys
            .AnyAsync(journey => journey.UserId == user.Id);

        if (alreadyEnrolled)
            return;

        _context.OnboardingJourneys.Add(new OnboardingJourney
        {
            UserId = user.Id,
            NextStep = 0,
            NextEmailAtUtc = _timeProvider.GetUtcNow().UtcDateTime
        });

        await _context.SaveChangesAsync();
    }

    public async Task CancelAsync(Guid userId, string reason)
    {
        var journey = await _context.OnboardingJourneys
            .SingleOrDefaultAsync(item => item.UserId == userId && item.CompletedAtUtc == null && item.CanceledAtUtc == null);

        if (journey == null)
            return;

        journey.CanceledAtUtc = _timeProvider.GetUtcNow().UtcDateTime;
        journey.CompletionReason = reason;
        await _context.SaveChangesAsync();
    }
}
