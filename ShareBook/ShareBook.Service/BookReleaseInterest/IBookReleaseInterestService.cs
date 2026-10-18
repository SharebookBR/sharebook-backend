using ShareBook.Domain.DTOs;
using System;
using System.Threading.Tasks;

namespace ShareBook.Service;

public interface IBookReleaseInterestService
{
    /// <summary>Total de avisos pedidos e, se houver pessoa logada, se ela já pediu.</summary>
    Task<ReleaseInterestStatusDTO> GetStatusAsync(Guid bookId, Guid? userId);

    /// <summary>Pessoa logada pede para ser avisada na liberação. Idempotente.</summary>
    Task<ReleaseInterestStatusDTO> SubscribeAsync(Guid bookId);

    /// <summary>Pessoa logada cancela o pedido de aviso. Idempotente.</summary>
    Task<ReleaseInterestStatusDTO> UnsubscribeAsync(Guid bookId);

    /// <summary>
    /// Envia o aviso de liberação para quem pediu, dos livros que já viraram Available.
    /// Pode rodar quantas vezes for preciso: só pega avisos ainda pendentes, então uma falha de
    /// e-mail é retomada na próxima execução sem duplicar quem já recebeu. Devolve quantos foram enviados.
    /// </summary>
    Task<int> NotifyReleasedBooksAsync();
}
