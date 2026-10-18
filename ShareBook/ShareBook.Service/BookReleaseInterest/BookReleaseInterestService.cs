using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Domain.DTOs;
using ShareBook.Domain.Enums;
using ShareBook.Domain.Exceptions;
using ShareBook.Repository;
using ShareBook.Service.Upload;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ShareBook.Service;

public class BookReleaseInterestService(
    ApplicationDbContext context,
    ICurrentUserAccessor currentUserAccessor,
    IBooksEmailService booksEmailService,
    IUserService userService,
    IUploadService uploadService,
    IConfiguration configuration,
    TimeProvider timeProvider,
    ILogger<BookReleaseInterestService>? logger = null) : IBookReleaseInterestService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ICurrentUserAccessor _currentUserAccessor = currentUserAccessor;
    private readonly IBooksEmailService _booksEmailService = booksEmailService;
    private readonly IUserService _userService = userService;
    private readonly IUploadService _uploadService = uploadService;
    private readonly IConfiguration _configuration = configuration;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly ILogger<BookReleaseInterestService> _logger = logger ?? NullLogger<BookReleaseInterestService>.Instance;

    public async Task<ReleaseInterestStatusDTO> GetStatusAsync(Guid bookId, Guid? userId)
    {
        if (!await _context.Books.AnyAsync(b => b.Id == bookId))
            throw new ShareBookException(ShareBookException.Error.NotFound);

        var total = await _context.BookReleaseInterests.CountAsync(i => i.BookId == bookId);
        var subscribed = userId.HasValue
            && await _context.BookReleaseInterests.AnyAsync(i => i.BookId == bookId && i.UserId == userId.Value);

        return new ReleaseInterestStatusDTO { Total = total, Subscribed = subscribed };
    }

    public async Task<ReleaseInterestStatusDTO> SubscribeAsync(Guid bookId)
    {
        var userId = _currentUserAccessor.RequireUserId();

        var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId)
            ?? throw new ShareBookException(ShareBookException.Error.NotFound);

        if (book.Status == BookStatus.Available)
            throw new ShareBookException("Este livro já está liberado. Você pode solicitá-lo agora.");

        if (book.Status != BookStatus.Scheduled)
            throw new ShareBookException("Este livro não está em pré-lançamento.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new ShareBookException(ShareBookException.Error.NotFound, "Usuário não encontrado.");

        if (!user.AllowSendingEmail)
            throw new ShareBookException("Você desativou o recebimento de e-mails no seu cadastro. Ative essa opção para pedir o aviso.");

        if (!await _context.BookReleaseInterests.AnyAsync(i => i.BookId == bookId && i.UserId == userId))
        {
            _context.BookReleaseInterests.Add(new BookReleaseInterest { BookId = bookId, UserId = userId });
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Duas requisições simultâneas da mesma pessoa: o índice único já garantiu que há um aviso só.
                _context.ChangeTracker.Clear();
                if (!await _context.BookReleaseInterests.AnyAsync(i => i.BookId == bookId && i.UserId == userId))
                    throw;
            }
        }

        return await GetStatusAsync(bookId, userId);
    }

    public async Task<ReleaseInterestStatusDTO> UnsubscribeAsync(Guid bookId)
    {
        var userId = _currentUserAccessor.RequireUserId();

        var interest = await _context.BookReleaseInterests
            .FirstOrDefaultAsync(i => i.BookId == bookId && i.UserId == userId);

        if (interest != null)
        {
            _context.BookReleaseInterests.Remove(interest);
            await _context.SaveChangesAsync();
        }

        return await GetStatusAsync(bookId, userId);
    }

    public async Task<int> NotifyReleasedBooksAsync()
    {
        var frontendUrl = _configuration["ServerSettings:FrontendUrl"] ?? "https://www.sharebook.com.br";

        var pending = await _context.BookReleaseInterests
            .Include(i => i.Book)
            .Include(i => i.User)
            .Where(i => i.NotifiedAt == null && i.Book.Status == BookStatus.Available)
            .ToListAsync();

        var sent = 0;
        foreach (var interest in pending)
        {
            var user = interest.User;
            var book = interest.Book;

            // Conta anonimizada, e-mail desativado depois do pedido ou endereço inválido: não há o que enviar.
            // Marca como tratado para não reavaliar a cada execução.
            if (!user.Active || !user.AllowSendingEmail || !EmailAddressValidator.IsValid(user.Email))
            {
                interest.NotifiedAt = _timeProvider.GetUtcNow().UtcDateTime;
                await _context.SaveChangesAsync();
                continue;
            }

            try
            {
                var coverUrl = _uploadService.GetImageUrl(book.ImageSlug ?? string.Empty, "Books", book.ImageVersion) ?? string.Empty;
                var token = _userService.GenerateUnsubscribeToken(user.Id);
                var unsubscribeUrl = $"{frontendUrl}/descadastrar?userId={user.Id}&token={token}";

                await _booksEmailService.SendEmailBookReleasedAsync(book, user, coverUrl, frontendUrl, unsubscribeUrl);

                interest.NotifiedAt = _timeProvider.GetUtcNow().UtcDateTime;
                await _context.SaveChangesAsync();
                sent++;
            }
            catch (Exception exception)
            {
                // Fica pendente (NotifiedAt nulo) e a próxima execução tenta de novo. Um e-mail ruim não trava os outros.
                _logger.LogError(exception, "Falha ao enfileirar o aviso de liberação do livro {BookId} para o usuário {UserId}.", book.Id, user.Id);
            }
        }

        return sent;
    }
}
