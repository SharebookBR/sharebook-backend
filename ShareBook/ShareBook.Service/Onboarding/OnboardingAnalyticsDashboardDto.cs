namespace ShareBook.Service.Onboarding;

public class OnboardingAnalyticsDashboardDto
{
    public int Days { get; set; }
    public int JourneysStarted { get; set; }
    public int JourneysCompleted { get; set; }
    public int JourneysCanceled { get; set; }
    public int OverdueJourneys { get; set; }
    public List<OnboardingStepMetricDto> Steps { get; set; } = [];
}

public class OnboardingStepMetricDto
{
    public int Step { get; set; }
    public string Label { get; set; } = string.Empty;
    public int Queued { get; set; }
    public int Opened { get; set; }
    public int Clicked { get; set; }
    public List<OnboardingCtaMetricDto> Ctas { get; set; } = [];
}

public class OnboardingCtaMetricDto
{
    public string Cta { get; set; } = string.Empty;
    public int Clicks { get; set; }
}
