using System;

namespace ShareBook.Domain.Common;

/// <summary>
/// Acessor único do usuário autenticado no contexto atual (request HTTP em andamento).
/// Substitui o padrão pré-ASP.NET-Core de copiar <c>HttpContext.User</c> para
/// <c>Thread.CurrentPrincipal</c> a cada request (ver <c>GetClaimsFilterAttribute</c>, removido
/// junto com esta abstração) — ASP.NET Core já expõe o usuário autenticado nativamente via DI.
/// </summary>
public interface ICurrentUserAccessor
{
    /// <summary>
    /// Id do usuário autenticado, ou null se não houver nenhum (ex.: job em background,
    /// endpoint anônimo, ou contexto sem requisição HTTP). Uso em código que não deve lançar
    /// quando não há usuário — ex.: auditoria de mudanças no banco.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Id do usuário autenticado. Lança <see cref="InvalidOperationException"/> se não houver
    /// usuário autenticado no contexto atual — uso em código de negócio que só é alcançável
    /// atrás de um endpoint protegido por autenticação (o mesmo comportamento, na prática, do
    /// antigo <c>new Guid(Thread.CurrentPrincipal?.Identity?.Name)</c>, só que com uma exceção
    /// que explica a causa em vez de um ArgumentNullException/FormatException genérico).
    /// </summary>
    Guid RequireUserId();
}
