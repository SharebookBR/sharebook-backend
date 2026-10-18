namespace ShareBook.Domain.DTOs;

public class ReleaseInterestStatusDTO
{
    /// <summary>Quantas pessoas pediram para ser avisadas sobre o livro.</summary>
    public int Total { get; set; }

    /// <summary>Se a pessoa logada pediu o aviso. Sempre false para visitante.</summary>
    public bool Subscribed { get; set; }
}
