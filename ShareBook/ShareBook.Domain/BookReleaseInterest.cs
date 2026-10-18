using ShareBook.Domain.Common;
using System;

namespace ShareBook.Domain;

/// <summary>
/// "Quero ser avisado": pessoa logada pediu um e-mail no dia em que um livro Scheduled for liberado.
/// Não é solicitação, reserva nem prioridade.
/// </summary>
public class BookReleaseInterest : BaseEntity
{
    public Guid BookId { get; set; }

    public Book Book { get; set; } = null!;

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    /// <summary>Quando o aviso de liberação foi entregue à fila de e-mail. Nulo = ainda pendente.</summary>
    public DateTime? NotifiedAt { get; set; }
}
