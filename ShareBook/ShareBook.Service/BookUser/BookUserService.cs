using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Domain.DTOs;
using ShareBook.Domain.Enums;
using ShareBook.Domain.Exceptions;
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

namespace ShareBook.Service
{
    public class BookUserService : BaseService<BookUser>, IBookUserService
    {
        private readonly IBookUserRepository _bookUserRepository;
        private readonly IBookService _bookService;
        private readonly IBookUsersEmailService _bookUsersEmailService;
        private readonly IMuambatorService _muambatorService;
        private readonly IBookRepository _bookRepository;
        private readonly IConfiguration _configuration;

        public BookUserService(
            IBookUserRepository bookUserRepository,
            IBookService bookService,
            IBookUsersEmailService bookUsersEmailService,
            IMuambatorService muambatorService,
            IBookRepository bookRepository,
            IUnitOfWork unitOfWork,
            IValidator<BookUser> validator, IConfiguration configuration)
            : base(bookUserRepository, unitOfWork, validator)
        {
            _bookUserRepository = bookUserRepository;
            _bookService = bookService;
            _bookUsersEmailService = bookUsersEmailService;
            _muambatorService = muambatorService;
            _bookRepository = bookRepository;
            _configuration = configuration;
        }

        public IList<User> GetGranteeUsersByBookId(Guid bookId) =>
            _bookUserRepository.Get().Include(x => x.User)
            .Where(x => x.BookId == bookId && x.Status == DonationStatus.WaitingAction)
            .Select(x => x.User.Cleanup()).ToList();

        public IList<BookUser> GetRequestersList(Guid bookId) =>
            _bookUserRepository.Get()
            .Include(x => x.User).ThenInclude(u => u.Address)
            .Include(x => x.User).ThenInclude(u => u.BookUsers)
            .Include(x => x.User).ThenInclude(u => u.BooksDonated)
            .Where(x => x.BookId == bookId)
            .OrderBy(x => x.CreationDate)
            .ToList(); // TODO: Migrate to async

        public async Task InsertAsync(Guid bookId, string reason)
        {
            //obtem o livro requisitado e o doador
            var bookRequested = await _bookService.GetBookWithAllUsersAsync(bookId);
            var bookUser = new BookUser()
            {
                BookId = bookId,
                UserId = new Guid(Thread.CurrentPrincipal?.Identity?.Name),
                Reason = reason,
                NickName = $"Interessado {bookRequested?.TotalInterested() + 1}"
            };

            if (!await _bookService.AnyAsync(x => x.Id == bookUser.BookId))
                throw new ShareBookException(ShareBookException.Error.NotFound);

            if (await _bookUserRepository.AnyAsync(x => x.UserId == bookUser.UserId && x.BookId == bookUser.BookId))
                throw new ShareBookException("O usuário já possui uma requisição para o mesmo livro.");

            if (bookRequested.Status != BookStatus.Available)
                throw new ShareBookException("Esse livro não está mais disponível para doação.");

            await _bookUserRepository.InsertAsync(bookUser);

            // Remove da vitrine caso o número de pedidos estiver grande demais.
            await MaxRequestsValidationAsync(bookRequested);

            await _bookUsersEmailService.SendEmailBookDonorAsync(bookUser, bookRequested);
            await _bookUsersEmailService.SendEmailBookInterestedAsync(bookUser, bookRequested);
            
        }

        private async Task MaxRequestsValidationAsync(Book bookRequested)
        {
            var maxRequestsPerBook = int.Parse(_configuration["SharebookSettings:MaxRequestsPerBook"]);
            if (bookRequested.BookUsers.Count < maxRequestsPerBook)
                return;

            bookRequested.Status = BookStatus.AwaitingDonorDecision;
            bookRequested.ChooseDate = DateTime.Today.AddDays(1);
            await _bookRepository.UpdateAsync(bookRequested);

            await _bookUsersEmailService.SendEmailMaxRequestsAsync(bookRequested);
        }

        public async Task DonateBookAsync(Guid bookId, Guid userId, string note)
        {
            var book = await _bookService.FindAsync(bookId);
            if (!book.MayChooseWinner())
                throw new ShareBookException(ShareBookException.Error.BadRequest, "Aguarde a data de decisão.");

            var bookUserAccepted = await _bookUserRepository.Get()
                .Include(u => u.Book).ThenInclude(b => b.UserFacilitator)
                .Include(u => u.Book).ThenInclude(b => b.User)
                .Include(u => u.User).ThenInclude(u => u.Address)
                .Where(x => x.UserId == userId
                    && x.BookId == bookId
                    && x.Status == DonationStatus.WaitingAction)
                    .FirstOrDefaultAsync();

            if (bookUserAccepted == null)
                throw new ShareBookException("Não existe a relação de usuário e livro para a doação.");

            if (bookUserAccepted.Status == DonationStatus.Canceled)
                throw new ShareBookException("O solicitante desistiu do seu pedido. Por favor escolha outro ganhador.");

            bookUserAccepted.UpdateBookUser(DonationStatus.Donated, note);

            await _bookUserRepository.UpdateAsync(bookUserAccepted);

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

        public async Task<Result<Book>> CancelAsync(BookCancelationDTO dto)
        {
            if (dto.Book == null)
                throw new ShareBookException(ShareBookException.Error.NotFound);

            // TODO: Verify if we can remove this call
            var bookUsers = await _bookUserRepository.Get().Where(x => x.BookId == dto.Book.Id).ToListAsync();

            dto.Book.ChooseDate = null;
            dto.Book.Status = BookStatus.Canceled;

            await CancelBookUsersAndSendNotificationAsync(dto.Book);

            await _bookService.UpdateAsync(dto.Book);
            await _bookUsersEmailService.SendEmailBookCanceledToAdminsAndDonorAsync(dto);

            return new Result<Book>(dto.Book);
        }

        public async Task DeniedBookUsersAsync(Guid bookId)
        {
            var bookUsersDenied = await _bookUserRepository.Get().Where(x => x.BookId == bookId
            && x.Status == DonationStatus.WaitingAction).ToListAsync();
            foreach (var item in bookUsersDenied)
            {
                string note = string.Empty;
                item.UpdateBookUser(DonationStatus.Denied, note);
                await _bookUserRepository.UpdateAsync(item);
            }
        }

        private async Task CancelBookUsersAndSendNotificationAsync(Book book)
        {
            await DeniedBookUsersAsync(book.Id);
            await NotifyUsersBookCanceledAsync(book);
        }

        public async Task<PagedList<BookUser>> GetRequestsByUserAsync(int page, int itemsPerPage)
        {
            var userId = new Guid(Thread.CurrentPrincipal?.Identity?.Name);
            var query = _bookUserRepository.Get()
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
            var bookUsers = await _bookUserRepository.Get()
                                                .Include(u => u.Book)
                                                .Include(u => u.User)
                                                .Where(x => x.BookId == bookId).ToListAsync();

            //obter apenas o ganhador
            var winnerBookUser = bookUsers.FirstOrDefault(bu => bu.Status == DonationStatus.Donated);

            //Book
            var book = winnerBookUser.Book;

            //usuarios que perderam a doação :(
            var losersBookUser = bookUsers.Where(bu => bu.Status == DonationStatus.Denied).ToList();

            //enviar e-mails
            await this._bookUsersEmailService.SendEmailDonationDeclinedAsync(book, winnerBookUser, losersBookUser);
        }

        public async Task NotifyUsersBookCanceledAsync(Book book)
        {
            List<BookUser> bookUsers = await _bookUserRepository.Get()
                                            .Include(u => u.User)
                                            .Where(x => x.BookId == book.Id).ToListAsync();

            await this._bookUsersEmailService.SendEmailDonationCanceledAsync(book, bookUsers);
        }

        public async Task InformTrackingNumberAsync(Guid bookId, string trackingNumber)
        {
            var book = await _bookRepository.Get()
                                      .Include(d => d.User)
                                      .Include(f => f.UserFacilitator)
                                      .FirstOrDefaultAsync(id => id.Id == bookId);
            var winnerBookUser = await _bookUserRepository
                                        .Get()
                                        .Include(u => u.User)
                                        .Where(bu => bu.BookId == bookId && bu.Status == DonationStatus.Donated)
                                        .FirstOrDefaultAsync();

            if (winnerBookUser == null)
                throw new ShareBookException("Vencedor ainda não foi escolhido");

            if (MuambatorConfigurator.IsActive)
                await _muambatorService.AddPackageToTrackerAsync(book, winnerBookUser.User, trackingNumber);

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
                await _bookUserRepository.UpdateAsync(request);

                return true;
            }

            return false;
        }
        public async Task<BookUser> GetRequestAsync(Guid requestId)
        {
            return await _bookUserRepository.FindAsync(new IncludeList<BookUser>(x => x.Book), x => x.Id == requestId);
        }
    }
}
