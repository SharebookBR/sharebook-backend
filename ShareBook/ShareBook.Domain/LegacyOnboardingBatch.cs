using ShareBook.Domain.Common;

namespace ShareBook.Domain;

/// <summary>
/// Uma remessa da inclusão gradual de contas anteriores ao onboarding.
/// </summary>
public class LegacyOnboardingBatch : BaseEntity
{
    public int SequenceNumber { get; set; }

    public DateTime ExecutedAtUtc { get; set; }

    public int EnrolledCount { get; set; }

    public DateTime? NewestUserCreationAtUtc { get; set; }

    public DateTime? OldestUserCreationAtUtc { get; set; }

    public ICollection<OnboardingJourney> Journeys { get; set; } = [];
}
