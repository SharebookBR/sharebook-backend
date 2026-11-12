namespace ShareBook.Service.Onboarding;

/// <summary>
/// Cadência da inclusão das contas criadas antes do onboarding existir.
/// O cursor persistido no histórico do job garante que cada lote continua
/// exatamente de onde o anterior terminou.
/// </summary>
public class LegacyOnboardingBackfillSettings
{
    public int BatchSize { get; set; } = 2000;

    /// <summary>Primeiro instante em que o onboarding normal passou a existir.</summary>
    public DateTime EligibleBeforeUtc { get; set; } = new(2026, 10, 23, 18, 26, 0, DateTimeKind.Utc);
}
