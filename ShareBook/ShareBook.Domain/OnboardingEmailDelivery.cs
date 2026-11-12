using ShareBook.Domain.Common;

namespace ShareBook.Domain;

/// <summary>
/// Evidência operacional de uma etapa da jornada de onboarding colocada na fila.
/// Não armazena e-mail, IP ou user-agent.
/// </summary>
public class OnboardingEmailDelivery : BaseEntity
{
    public Guid OnboardingJourneyId { get; set; }

    public OnboardingJourney OnboardingJourney { get; set; } = null!;

    /// <summary>Etapa entregue, de 0 a 4.</summary>
    public int Step { get; set; }

    public DateTime QueuedAtUtc { get; set; }

    public DateTime? FirstOpenedAtUtc { get; set; }

    public ICollection<OnboardingEmailClick> Clicks { get; set; } = [];
}
