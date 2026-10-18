using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Domain.Common;
using ShareBook.Domain.DTOs;
using ShareBook.Service;
using System;
using System.Threading.Tasks;

namespace ShareBook.Api.Controllers;

/// <summary>"Quero ser avisado" dos livros em pré-lançamento (status Scheduled).</summary>
[Route("api/[controller]")]
public class BookReleaseInterestController(IBookReleaseInterestService service, ICurrentUserAccessor currentUserAccessor) : ControllerBase
{
    private readonly IBookReleaseInterestService _service = service;
    private readonly ICurrentUserAccessor _currentUserAccessor = currentUserAccessor;

    /// <summary>Aberto a visitantes: só o total de pessoas que pediram aviso.</summary>
    [HttpGet("{bookId:guid}/Count")]
    [ProducesResponseType(typeof(ReleaseInterestStatusDTO), 200)]
    public async Task<ReleaseInterestStatusDTO> CountAsync(Guid bookId)
        => await _service.GetStatusAsync(bookId, null);

    [Authorize("Bearer")]
    [HttpGet("{bookId:guid}")]
    [ProducesResponseType(typeof(ReleaseInterestStatusDTO), 200)]
    public async Task<ReleaseInterestStatusDTO> GetAsync(Guid bookId)
        => await _service.GetStatusAsync(bookId, _currentUserAccessor.RequireUserId());

    [Authorize("Bearer")]
    [HttpPost("{bookId:guid}")]
    [ProducesResponseType(typeof(ReleaseInterestStatusDTO), 200)]
    public async Task<ReleaseInterestStatusDTO> SubscribeAsync(Guid bookId)
        => await _service.SubscribeAsync(bookId);

    [Authorize("Bearer")]
    [HttpDelete("{bookId:guid}")]
    [ProducesResponseType(typeof(ReleaseInterestStatusDTO), 200)]
    public async Task<ReleaseInterestStatusDTO> UnsubscribeAsync(Guid bookId)
        => await _service.UnsubscribeAsync(bookId);
}
