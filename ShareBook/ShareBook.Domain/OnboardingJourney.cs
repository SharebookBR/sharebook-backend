using ShareBook.Domain.Common;

namespace ShareBook.Domain;

/// <summary>
/// Jornada de boas-vindas iniciada no cadastro. Cada etapa é entregue no máximo uma vez.
/// </summary>
public class OnboardingJourney : BaseEntity
{
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    /// <summary>Próxima etapa a enviar, de 0 a 4.</summary>
    public int NextStep { get; set; }

    public DateTime NextEmailAtUtc { get; set; }

    /// <summary>Última etapa entregue à fila de e-mail.</summary>
    public DateTime? LastQueuedAtUtc { get; set; }

    /// <summary>Fim normal da jornada após a quinta etapa ou por ativação.</summary>
    public DateTime? CompletedAtUtc { get; set; }

    public DateTime? CanceledAtUtc { get; set; }

    public string? CompletionReason { get; set; }
}
