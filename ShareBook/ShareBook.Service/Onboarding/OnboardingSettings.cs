namespace ShareBook.Service.Onboarding;

public class OnboardingSettings
{
    /// <summary>URL de uma comunidade já moderada. Vazia = o quinto e-mail fecha no próprio Sharebook.</summary>
    public string CommunityUrl { get; set; } = string.Empty;

    /// <summary>Limita o lote para respeitar a cadência do MailSender.</summary>
    public int MaxJourneysPerRun { get; set; } = 20;
}
