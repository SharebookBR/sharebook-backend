namespace ShareBook.Service.Onboarding;

public class LegacyOnboardingRolloutDashboardDto
{
    public int BatchSize { get; set; }
    public int RemainingEligibleJourneys { get; set; }
    public int RemainingBatches { get; set; }
    public DateTime? NextBatchAtUtc { get; set; }
    public DateTime? EstimatedFinalBatchAtUtc { get; set; }
    public List<LegacyOnboardingBatchDto> Batches { get; set; } = [];
    public List<LegacyOnboardingUpcomingBatchDto> UpcomingBatches { get; set; } = [];
}

public class LegacyOnboardingBatchDto
{
    public Guid Id { get; set; }
    public int SequenceNumber { get; set; }
    public DateTime ExecutedAtUtc { get; set; }
    public int EnrolledCount { get; set; }
    public int CurrentJourneys { get; set; }
    public DateTime? NewestUserCreationAtUtc { get; set; }
    public DateTime? OldestUserCreationAtUtc { get; set; }
    public int AgeInDays { get; set; }
    public bool IsComparable { get; set; }
    public LegacyOnboardingBatchMetricsDto CurrentMetrics { get; set; } = new();
    public LegacyOnboardingBatchMetricsDto? FirstSevenDaysMetrics { get; set; }
}

public class LegacyOnboardingUpcomingBatchDto
{
    public int SequenceNumber { get; set; }
    public DateTime ScheduledAtUtc { get; set; }
    public int EstimatedJourneys { get; set; }
}

public class LegacyOnboardingBatchMetricsDto
{
    public int FirstEmailQueued { get; set; }
    public int FirstEmailOpened { get; set; }
    public int FirstEmailClicked { get; set; }
    public int JourneysCompleted { get; set; }
    public int JourneysCanceled { get; set; }
}
