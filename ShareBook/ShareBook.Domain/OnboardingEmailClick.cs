using ShareBook.Domain.Common;

namespace ShareBook.Domain;

/// <summary>Primeiro clique rastreado de um CTA em uma etapa do onboarding.</summary>
public class OnboardingEmailClick : BaseEntity
{
    public Guid OnboardingEmailDeliveryId { get; set; }

    public OnboardingEmailDelivery OnboardingEmailDelivery { get; set; } = null!;

    public string Cta { get; set; } = string.Empty;

    public DateTime FirstClickedAtUtc { get; set; }
}
