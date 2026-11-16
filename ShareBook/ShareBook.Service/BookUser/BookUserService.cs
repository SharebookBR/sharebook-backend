using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Domain.DTOs;
using ShareBook.Domain.Enums;
using ShareBook.Domain.Exceptions;
using ShareBook.Helper;
using ShareBook.Repository;
using ShareBook.Repository.Repository;
using ShareBook.Repository.UoW;
using ShareBook.Service.Generic;
using ShareBook.Service.Muambator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShareBook.Service;

public class BookUserService(
    ApplicationDbContext context,
    IBookService bookService,
    IBookUsersEmailService bookUsersEmailService,
    IMuambatorService muambatorService,
    IBookRepository bookRepository,
    IUnitOfWork unitOfWork,
    IValidator<BookUser> validator, IConfiguration configuration, ICurrentUserAccessor currentUserAccessor, ILogger<BookUserService>? logger = null,
    TimeProvider? timeProvider = null) : BaseService<BookUser>(context, unitOfWork, validator), IBookUserService
{
    private readonly ApplicationDbContext _context = context;
    private readonly IBookService _bookService = bookService;
    private readonly IBookUsersEmailService _bookUsersEmailService = bookUsersEmailService;
    private readonly IMuambatorService _muambatorService = muambatorService;
    private readonly IBookRepository _bookRepository = bookRepository;
    private readonly IConfiguration _configuration = configuration;
    private readonly ICurrentUserAccessor _currentUserAccessor = currentUserAccessor;
    private readonly ILogger<BookUserService> _logger = logger ?? NullLogger<BookUserService>.Instance;
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

    public async Task<IList<User>> GetGranteeUsersByBookIdAsync(Guid bookId) =>
        await _repository.Get().Include(x => x.User)
        .Where(x => x.BookId == bookId && x.Status == DonationStatus.WaitingAction)
        .Select(x => x.User.Cleanup()).ToListAsync();

    public async Task<IList<BookUser>> GetRequestersListAsync(Guid bookId) =>
        await _repository.Get()
            .Include(x => x.User).ThenInclude(u => u.Address)
            .Include(x => x.User).ThenInclude(u => u.BookUsers)
            .Include(x => x.User).ThenInclude(u => u.BooksDonated)
            .Where(x => x.BookId == bookId)
            .OrderBy(x => x.CreationDate)
            .ToListAsync();

    public async Task InsertAsync(Guid bookId, string reason)
    {
        await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        Book? bookRequested = null;
        BookUser? bookUser = null;
        var reachedMaxRequests = false;

        try
        {
            // Read, insert and limit validation must share the same serializable transaction.
            bookRequested = await _bookService.GetBookWithAllUsersAsync(bookId);
            if (bookRequested == null)
                throw new ShareBookException(ShareBookException.Error.NotFound);

            bookUser = new BookUser()
            {
                BookId = bookId,
                UserId = _currentUserAccessor.RequireUserId(),
                Reason = reason,
                NickName = $"Interessado {bookRequested.TotalInterested() + 1}"
            };

            if (await _repository.AnyAsync(x => x.UserId == bookUser.UserId && x.BookId == bookUser.BookId))
                throw new ShareBookException("O usuário já possui uma requisição para o mesmo livro.");

            if (bookRequested.Status == BookStatus.Scheduled)
                throw new ShareBookException("Este livro ainda não foi liberado para solicitação.");

            if (bookRequested.Status != BookStatus.Available)
                throw new ShareBookException("Esse livro não está mais disponível para doação.");

            await _repository.InsertAsync(bookUser);

            // Remove da vitrine caso o número de pedidos estiver grande demais.
            reachedMaxRequests = await MaxRequestsValidationAsync(bookRequested);
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }

        await _bookUsersEmailService.SendEmailBookDonorAsync(bookUser!, bookRequested!);
        await _bookUsersEmailService.SendEmailBookInterestedAsync(bookUser!, bookRequested!);

        if (reachedMaxRequests)
            await _bookUsersEmailService.SendEmailMaxRequestsAsync(bookRequested!);
    }

    private async Task<bool> MaxRequestsValidationAsync(Book bookRequested)
    {
        if (bookRequested.IsEbook())
            return false;

        var requestCount = await _context.BookUser.CountAsync(x => x.BookId == bookRequested.Id);
        var maxRequests = BookRequestLimit.GetEffective(bookRequested, _configuration);
        if (requestCount < maxRequests)
            return false;

        bookRequested.Status = BookStatus.AwaitingDonorDecision;
        bookRequested.ChooseDate = DateTime.Today.AddDays(1);
        await _bookRepository.UpdateAsync(bookRequested);

        return true;
    }

    public async Task DonateBookAsync(Guid bookId, Guid userId, string note)
    {
        var book = await _bookService.FindAsync(bookId);
        if (book == null)
            throw new ShareBookException(ShareBookException.Error.NotFound);

        if (!book.MayChooseWinner())
            throw new ShareBookException(ShareBookException.Error.BadRequest, "Aguarde a data de decisão.");

        var bookUserAccepted = await _repository.Get()
            .Include(u => u.Book).ThenInclude(b => b.User)
            .Include(u => u.User).ThenInclude(u => u.Address)
            .Where(x => x.UserId == userId
                && x.BookId == bookId
                && x.Status == DonationStatus.WaitingAction)
                .FirstOrDefaultAsync();

        if (bookUserAccepted == null)
            throw new ShareBookException("Não existe a relação de usuário e livro para a doação.");

        if (bookUserAccepted.Status == DonationStatus.Canceled)
            throw new ShareBookException("A pessoa solicitante desistiu do pedido. Escolha outra pessoa para receber o livro.");

        bookUserAccepted.UpdateBookUser(DonationStatus.Donated, note);

        await _repository.UpdateAsync(bookUserAccepted);

        await DeniedBookUsersAsync(bookId);

        await _bookService.UpdateBookStatusAsync(bookId, BookStatus.WaitingSend);

        // usamos await nas notificações porque eventualmente tem risco da taks
        // não completar o trabalho dela. Talvez tenha a ver com o garbage collector.

        // avisa o ganhador
        await _bookUsersEmailService.SendEmailBookDonatedAsync(bookUserAccepted);

        // avisa os perdedores :/
        await NotifyInterestedAboutBooksWinnerAsync(bookId);

        // avisa o doador
        await _bookUsersEmailService.SendEmailBookDonatedNotifyDonorAsync(bookUserAccepted.Book, bookUserAccepted.User);
    }

    public async Task ChangeStatusAsync(Guid bookId, BookStatus newStatus, string reason, string adminName)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ShareBookException(ShareBookException.Error.BadRequest, "Informe o motivo interno para alterar o status.");

        await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        BookUser? previousWinner = null;

        try
        {
            var book = await _context.Books
                .Include(x => x.BookUsers).ThenInclude(x => x.User)
                .SingleOrDefaultAsync(x => x.Id == bookId);

            if (book == null)
                throw new ShareBookException(ShareBookException.Error.NotFound);

            if (book.Status != BookStatus.WaitingSend || newStatus != BookStatus.AwaitingDonorDecision)
                throw new ShareBookException(ShareBookException.Error.BadRequest, "Por enquanto, apenas a transição de Aguardando envio para Aguardando decisão da pessoa doadora é permitida.");

            previousWinner = book.BookUsers.SingleOrDefault(x => x.Status == DonationStatus.Donated);
            if (previousWinner == null)
                throw new ShareBookException(ShareBookException.Error.BadRequest, "Não há uma pessoa ganhadora para substituir nesta doação.");

            var otherRequesters = book.BookUsers
                .Where(x => x.Status == DonationStatus.Denied)
                .ToList();

            if (otherRequesters.Count == 0)
                throw new ShareBookException(ShareBookException.Error.BadRequest, "Não há outra pessoa interessada para escolher nesta doação.");

            previousWinner.UpdateBookUser(DonationStatus.SelectionReopened, previousWinner.Note);
            foreach (var requester in otherRequesters)
                requester.UpdateBookUser(DonationStatus.WaitingAction, requester.Note);

            book.Status = BookStatus.AwaitingDonorDecision;
            book.ChooseDate = _timeProvider.GetUtcNow().UtcDateTime;

            var saoPauloNow = DateTimeHelper.ConvertDateTimeSaoPaulo(_timeProvider.GetUtcNow().UtcDateTime);
            var entry = $"{saoPauloNow:dd/MM/yyyy HH:mm} - Status alterado por {adminName}: Aguardando envio → Aguardando decisão da pessoa doadora. Motivo: {reason.Trim()}";
            book.AdminNotes = string.IsNullOrWhiteSpace(book.AdminNotes)
                ? entry
                : $"{book.AdminNotes}\n{entry}";

            await _context.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }

        await _bookUsersEmailService.SendEmailWinnerSelectionReopenedAsync(previousWinner!);
    }

    public async Task<Result<Book>> CancelAsync(BookCancelationDTO dto)
    {
        if (dto.Book == null)
            throw new ShareBookException(ShareBookException.Error.NotFound);

        dto.Book.ChooseDate = null;
        dto.Book.Status = BookStatus.Canceled;

        await CancelBookUsersAndSendNotificationAsync(dto.Book);

        await _bookService.UpdateAsync(dto.Book);
        await _bookUsersEmailService.SendEmailBookCanceledToAdminsAndDonorAsync(dto);

        return new Result<Book>(dto.Book);
    }

    public async Task DeniedBookUsersAsync(Guid bookId)
    {
        var bookUsersDenied = await _repository.Get().Where(x => x.BookId == bookId
        && x.Status == DonationStatus.WaitingAction).ToListAsync();
        foreach (var item in bookUsersDenied)
        {
            string note = string.Empty;
            item.UpdateBookUser(DonationStatus.Denied, note);
            await _repository.UpdateAsync(item);
        }
    }

    private async Task CancelBookUsersAndSendNotificationAsync(Book book)
    {
        await DeniedBookUsersAsync(book.Id);
        await NotifyUsersBookCanceledAsync(book);
    }

    public async Task<PagedList<BookUser>> GetRequestsByUserAsync(int page, int itemsPerPage)
    {
        var userId = _currentUserAccessor.RequireUserId();
        var query = _repository.Get()
            .Include(x => x.Book)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreationDate);

        var result = await FormatPagedListAsync(query, page, itemsPerPage);

        // só mostra o código de rastreio se ele for o ganhador.
        result.Items = result.Items.Select(bu =>
        {
            bu.Book.TrackingNumber = bu.Status == DonationStatus.Donated ? bu.Book.TrackingNumber : null;
            return bu;
        }).ToList();

        return result;
    }

    public async Task NotifyInterestedAboutBooksWinnerAsync(Guid bookId)
    {
        //Obter todos os users do livro
        var bookUsers = await _repository.Get()
                                            .Include(u => u.Book)
                                            .Include(u => u.User)
                                            .Where(x => x.BookId == bookId).ToListAsync();

        //obter apenas o ganhador
        var winnerBookUser = bookUsers.FirstOrDefault(bu => bu.Status == DonationStatus.Donated);
        if (winnerBookUser == null)
            throw new ShareBookException(ShareBookException.Error.NotFound, "Nenhuma pessoa ganhadora encontrada para este livro.");

        //Book
        var book = winnerBookUser.Book;

        //usuarios que perderam a doação :(
        var losersBookUser = bookUsers.Where(bu => bu.Status == DonationStatus.Denied).ToList();

        //enviar e-mails
        await this._bookUsersEmailService.SendEmailDonationDeclinedAsync(book, winnerBookUser, losersBookUser);
    }

    public async Task NotifyUsersBookCanceledAsync(Book book)
    {
        List<BookUser> bookUsers = await _repository.Get()
                                        .Include(u => u.User)
                                        .Where(x => x.BookId == book.Id).ToListAsync();

        await this._bookUsersEmailService.SendEmailDonationCanceledAsync(book, bookUsers);
    }

    public async Task InformTrackingNumberAsync(Guid bookId, string trackingNumber)
    {
        var book = await _bookRepository.Get()
                                  .Include(d => d.User)
                                  .FirstOrDefaultAsync(id => id.Id == bookId);
        var winnerBookUser = await _repository
                                    .Get()
                                    .Include(u => u.User)
                                    .Where(bu => bu.BookId == bookId && bu.Status == DonationStatus.Donated)
                                    .FirstOrDefaultAsync();

        if (winnerBookUser == null)
            throw new ShareBookException("A pessoa ganhadora ainda não foi escolhida.");

        if (book == null)
            throw new ShareBookException(ShareBookException.Error.NotFound);

        if (MuambatorConfigurator.IsActive)
        {
            try
            {
                await _muambatorService.AddPackageToTrackerAsync(book, winnerBookUser.User, trackingNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao enviar rastreio para o Muambator. Livro: {BookId}, Rastreio: {TrackingNumber}", bookId, trackingNumber);
            }
        }

        book.Status = BookStatus.Sent;
        book.TrackingNumber = trackingNumber;
        await _bookService.UpdateAsync(book);

        if (winnerBookUser.User.AllowSendingEmail)
            //Envia e-mail para avisar o ganhador do tracking number                          
            await _bookUsersEmailService.SendEmailTrackingNumberInformedAsync(winnerBookUser, book);
    }

    /// <summary>
    /// Cancel a request for a book if it's still awaiting for donor decision and not already canceled.
    /// </summary>
    /// <param name="request">The request to be canceled</param>
    /// <returns>Returns true if the operation gets executed successfully</returns>
    public async Task<bool> CancelRequestAsync(BookUser request)
    {
        if (request.Book.Status == BookStatus.AwaitingDonorDecision || request.Book.Status == BookStatus.Available && request.Status != DonationStatus.Canceled)
        {
            request.UpdateBookUser(DonationStatus.Canceled, String.Empty);
            request.Reason = "Pedido cancelado! Favor ignorar.";
            await _repository.UpdateAsync(request);

            return true;
        }

        return false;
    }
    public async Task<BookUser?> GetRequestAsync(Guid requestId)
    {
        return await _repository.FindAsync(new IncludeList<BookUser>(x => x.Book), x => x.Id == requestId);
    }
}
