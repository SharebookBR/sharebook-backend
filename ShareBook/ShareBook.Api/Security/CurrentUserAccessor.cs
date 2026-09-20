using Microsoft.AspNetCore.Http;
using ShareBook.Domain.Common;
using System;

namespace ShareBook.Api.Security;

/// <summary>
/// Implementação de <see cref="ICurrentUserAccessor"/> baseada em <see cref="IHttpContextAccessor"/>.
/// O Id do usuário é o "Name" da claim principal — mesmo dado que
/// <c>Thread.CurrentPrincipal?.Identity?.Name</c> carregava antes, só que lido direto do
/// <see cref="HttpContext"/> atual em vez de uma cópia manual pra thread estática.
/// </summary>
public class CurrentUserAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUserAccessor
{
    public Guid? UserId
    {
        get
        {
            var name = httpContextAccessor.HttpContext?.User?.Identity?.Name;
            return string.IsNullOrEmpty(name) ? null : Guid.Parse(name);
        }
    }

    public Guid RequireUserId()
        => UserId ?? throw new InvalidOperationException(
            "Nenhum usuário autenticado no contexto atual. Este código só deveria ser alcançável atrás de um endpoint protegido por autenticação.");
}
