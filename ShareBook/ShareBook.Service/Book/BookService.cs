using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Domain.DTOs;
using ShareBook.Domain.Enums;
using ShareBook.Domain.Exceptions;
using ShareBook.Helper;
using ShareBook.Helper.Extensions;
using ShareBook.Helper.Image;
using ShareBook.Repository;
using ShareBook.Repository.UoW;
using ShareBook.Service.AwsSqs;
using ShareBook.Service.AwsSqs.Dto;
using ShareBook.Service.Generic;
using ShareBook.Service.Upload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public class BookService : BaseService<Book>, IBookService
    {
        private readonly IUploadService _uploadService;
        private readonly IBooksEmailService _booksEmailService;
        private readonly IConfiguration _configuration;

        private readonly NewBookQueue _newBookQueue;

        public BookService(IBookRepository bookRepository,
                    IUnitOfWork unitOfWork, IValidator<Book> validator,
                    IUploadService uploadService, IBooksEmailService booksEmailService, IConfiguration configuration,
                    NewBookQueue newBookQueue)
                    : base(bookRepository, unitOfWork, validator)
        {
            _uploadService = uploadService;
            _booksEmailService = booksEmailService;
            _configuration = configuration;
            _newBookQueue = newBookQueue;
        }

        public async Task ApproveAsync(Guid bookId, DateTime? chooseDate = null)
        {
            var daysInShowcase = int.Parse(_configuration["SharebookSettings:DaysInShowcase"]);

            var book = await _repository.Get().Include(b => b.Category).FirstOrDefaultAsync(b => b.Id == bookId);
            if (book == null)
                throw new ShareBookException(ShareBookException.Error.NotFound);

            book.Status = BookStatus.Available;
            book.ChooseDate = chooseDate?.Date ?? DateTime.Today.AddDays(daysInShowcase);
            await _repository.UpdateAsync(book);

            // notifica o doador
            await _booksEmailService.SendEmailBookApprovedAsync(book);

            // notifica possíveis interessados.
            var message = new NewBookBody
            {
                BookId = book.Id,
                BookTitle = book.Title,
                CategoryId = book.CategoryId
            };
            await _newBookQueue.SendMessageAsync(message);

        }

        public async Task ReceivedAsync(Guid bookId, Guid winnerUserId)
        {
            var book = await _repository.Get().Include(f => f.BookUsers)
                .ThenInclude(bu => bu.User)
                .FirstOrDefaultAsync(f => f.Id == bookId);

            if (book == null)
                throw new ShareBookException(ShareBookException.Error.NotFound);

            // Verifica se o usuario é realmente o ganhador do livro
            var winner = book.WinnerUser();
            if (winner == null || winner.Id != winnerUserId)
                throw new ShareBookException(ShareBookException.Error.Forbidden);

            book.Status = BookStatus.Received;
            await _repository.UpdateAsync(book);

            await _booksEmailService.SendEmailBookReceivedAsync(book);
        }

        public async Task UpdateBookStatusAsync(Guid bookId, BookStatus bookStatus)
        {
            var book = await _repository.FindAsync(bookId);
            if (book == null)
                throw new ShareBookException(ShareBookException.Error.NotFound);

            book.Status = bookStatus;
            await _repository.UpdateAsync(book);
        }

        public IList<dynamic> FreightOptions()
        {
            var enumValues = new List<dynamic>();
            foreach (FreightOption freightOption in Enum.GetValues(typeof(FreightOption)))
            {
                enumValues.Add(new
                {
                    Value = freightOption.ToString(),
                    Text = freightOption.Description()
                });
            }
            return enumValues;
        }

        public async Task<IList<Book>> AvailableBooksAsync()
        {
            return SetImageUrl(
                await _repository.Get()
                    .Include(b => b.User)
                    .ThenInclude(u => u.Address)
                    .Include(b => b.Category)
                    .Where(b => b.Status == BookStatus.Available)
                    .OrderByDescending(b => b.CreationDate)
                    .ToListAsync()
            );
        }

        public async Task<IList<Book>> Random15BooksAsync()
        {
            return SetImageUrl(
                await _repository.Get()
                    .Include(b => b.User)
                    .ThenInclude(u => u.Address)
                    .Include(b => b.Category)
                    .Where(b => b.Status == BookStatus.Available)
                    .OrderBy(x => Guid.NewGuid()) // ordem aleatória
                    .Take(15) // apenas 15 registros
                    .ToListAsync()
             );
        }

        public async Task<IList<Book>> Random15EBooksAsync()
        {
            return SetImageUrl(
                await _repository.Get()
                    .Include(b => b.User)
                    .ThenInclude(u => u.Address)
                    .Include(b => b.Category)
                    .Where(b => b.Status == BookStatus.Available && b.Type == BookType.Eletronic)
                    .OrderBy(x => Guid.NewGuid()) // ordem aleatória
                    .Take(15) // apenas 15 registros
                    .ToListAsync()
             );
        }

        private IList<Book> SetImageUrl(IList<Book> books)
        {
            return books.Select(b => { b.ImageUrl = _uploadService.GetImageUrl(b.ImageSlug, "Books"); return b; }).ToList();
        }


        public async Task<IList<Book>> GetAllAsync(int page, int items)
            => await _repository.Get()
                .Include(b => b.User)
                .Include(b => b.BookUsers)
                .OrderBy(b => b.Id)
                .Skip((page - 1) * items)
                .Take(items).ToListAsync();

        public override async Task<Book> FindAsync(object keyValue)
        {
            var result = await _repository.Get()
                .Include(b => b.User)
                .ThenInclude(u => u.Address)
                .Include(b => b.Category)
                .Include(b => b.UserFacilitator)
                .Where(b => b.Id == (Guid)keyValue)
                .FirstOrDefaultAsync();

            if (result == null)
                throw new ShareBookException(ShareBookException.Error.NotFound);

            result.ImageUrl = _uploadService.GetImageUrl(result.ImageSlug, "Books");

            return result;
        }

        public override async Task<Result<Book>> InsertAsync(Book entity)
        {
            entity.UserId = new Guid(Thread.CurrentPrincipal?.Identity?.Name);

            EBookValidate(entity);

            var result = await ValidateAsync(entity);
            if (result.Success)
            {
                entity.Slug = SetSlugByTitleOrIncremental(entity);

                entity.ImageSlug = ImageHelper.FormatImageName(entity.ImageName, entity.Slug);

                if (entity.IsEbookPdfValid())
                    entity.EBookPdfFile = await _uploadService.UploadPdfAsync(entity.EBookPdfBytes, entity.EBookPdfFile, "EBooks");

                result.Value = await _repository.InsertAsync(entity);

                result.Value.ImageUrl = await _uploadService.UploadImageAsync(entity.ImageBytes, entity.ImageSlug, "Books");

                result.Value.ImageBytes = null;

                await _booksEmailService.SendEmailNewBookInsertedAsync(entity);
            }
            return result;
        }

        public override async Task<Result<Book>> UpdateAsync(Book entity)
        {
            Result<Book> result = Validate(entity, x =>
                x.Title,
                x => x.Author,
                x => x.FreightOption,
                x => x.Id);

            var bookId = entity.Id;

            if (!result.Success) return result;

            //buscar o book no banco para obter um objeto para ser re-hidratado
            var savedBook = await this._repository.FindAsync(bookId);

            if (savedBook == null)
                throw new ShareBookException(ShareBookException.Error.NotFound);

            EBookValidate(entity);

            //imagem eh opcional no update
            if (!string.IsNullOrEmpty(entity.ImageName) && entity.ImageBytes.Length > 0)
            {
                entity.ImageSlug = ImageHelper.FormatImageName(entity.ImageName, savedBook.Slug);
                await _uploadService.UploadImageAsync(entity.ImageBytes, savedBook.ImageSlug, "Books");
            }

            //preparar o book para atualização
            savedBook.Author = entity.Author;
            savedBook.FreightOption = entity.FreightOption;
            savedBook.Author = entity.Author;
            savedBook.ImageSlug = entity.ImageSlug;
            savedBook.Title = entity.Title;
            savedBook.CategoryId = entity.CategoryId;

            // Condição efetuada para evitar busca no BD desnecessariamente por conta do SetSlugByTitleOrIncremental()
            if (savedBook.Slug != entity.Slug)
                savedBook.Slug = SetSlugByTitleOrIncremental(entity);


            savedBook.Synopsis = entity.Synopsis;
            savedBook.TrackingNumber = entity.TrackingNumber;

            if (entity.UserIdFacilitator.HasValue && entity.UserIdFacilitator != Guid.Empty)
                savedBook.UserIdFacilitator = entity.UserIdFacilitator;

            result.Value = await _repository.UpdateAsync(savedBook);
            result.Value.ImageBytes = null;

            return result;
        }

        public async Task<PagedList<Book>> FullSearchAsync(string criteria, int page, int itemsPerPage, bool isAdmin)
        {
            Expression<Func<Book, bool>> filter = x => (x.Author.Contains(criteria)
                                                        || x.Title.Contains(criteria)
                                                        || x.Category.Name.Contains(criteria))
                                                        && x.Status == BookStatus.Available;

            if (!isAdmin) filter = x => x.Author.Contains(criteria)
                                        || x.Title.Contains(criteria)
                                        || x.Category.Name.Contains(criteria);

            return await SearchBooksAsync(filter, page, itemsPerPage);
        }

        public async Task<PagedList<Book>> ByCategoryIdAsync(Guid categoryId, int page, int itemsPerPage)
            => await SearchBooksAsync(x => x.Status == BookStatus.Available && x.CategoryId == categoryId, page, itemsPerPage);

        public async Task<Book> BySlugAsync(string slug)
        {
            var pagedBook = await SearchBooksAsync(x => (x.Slug.Equals(slug)), 1, 1);
            return pagedBook.Items.FirstOrDefault();
        }

        public async Task<bool> UserRequestedBookAsync(Guid bookId)
        {
            var userId = new Guid(Thread.CurrentPrincipal?.Identity?.Name);
            return await _repository.AnyAsync(x =>
                    x.Id == bookId &&
                    x.BookUsers
                    .Any(y =>
                    y.Status == DonationStatus.WaitingAction
                    && y.UserId == userId
              ));
        }

        public override async Task<PagedList<Book>> GetAsync<TKey>(Expression<Func<Book, bool>> filter, Expression<Func<Book, TKey>> order, int page, int itemsPerPage)
            => await base.GetAsync(filter, order, page, itemsPerPage);

        public async Task<IList<Book>> GetUserDonationsAsync(Guid userId)
        {
            return await _repository.Get()
                .Include(b => b.BookUsers)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreationDate)
                .ToListAsync();
        }

        public async Task<IList<Book>> GetBooksChooseDateIsTodayAsync()
        {
            // limite é o dia de hoje.
            DateTime startDateTime = DateTime.Today; //Today at 00:00:00
            DateTime endDateTime = DateTime.Today.AddDays(1).AddTicks(-1); //Today at 23:59:59

            // livros em que o choosedate é hoje.
            var books = await _repository
                .Get().Include(x => x.User).Include(x => x.BookUsers).Include(x => x.UserFacilitator)
                .Where(x =>
                    x.ChooseDate >= startDateTime &&
                    x.ChooseDate <= endDateTime
                ).ToListAsync();

            return books;
        }

        public async Task<IList<Book>> GetBooksChooseDateIsLateAsync()
        {
            DateTime today = DateTime.Today;

            var booksLate = await _repository
                .Get().Include(x => x.User).Include(x => x.BookUsers).Include(x => x.UserFacilitator)
                .Where(x => x.ChooseDate < today && x.Status == BookStatus.AwaitingDonorDecision)
                .OrderBy(x => x.ChooseDate)
                .ToListAsync();

            return booksLate;
        }

        /// <summary>
        /// Bom para remover o livro da vitrine.
        /// </summary>
        /// <returns></returns>
        public async Task<IList<Book>> GetBooksChooseDateIsTodayOrLateAsync()
        {
            // limite é o dia de hoje.
            DateTime endDateTime = DateTime.Today.AddDays(1).AddTicks(-1); //Today at 23:59:59

            // livros em que o choosedate é hoje.
            var books = await _repository
                .Get().Include(x => x.User).Include(x => x.BookUsers).Include(x => x.UserFacilitator)
                .Where(x =>
                    x.ChooseDate <= endDateTime && x.Status == BookStatus.Available
                ).ToListAsync();

            return books;
        }

        public async Task AddFacilitatorNotesAsync(Guid bookId, string facilitatorNotes)
        {
            var book = await _repository.FindAsync(bookId);
            if (book == null)
                throw new ShareBookException(ShareBookException.Error.NotFound);

            var saoPauloNow = DateTimeHelper.ConvertDateTimeSaoPaulo(DateTime.UtcNow);
            var date = saoPauloNow.ToString("dd/MM/yyyy");
            var lineBreak = (string.IsNullOrEmpty(book.FacilitatorNotes)) ? "" : "\n";
            book.FacilitatorNotes += string.Format("{0}{1} - {2}", lineBreak, date, facilitatorNotes);

            await _repository.UpdateAsync(book);
        }

        public async Task<Book> GetBookWithAllUsersAsync(Guid bookId)
        {
            return await _repository
                .Get().Include(x => x.User).ThenInclude(u => u.Address)
                .Include(x => x.UserFacilitator).ThenInclude(u => u.Address)
                .Include(x => x.BookUsers).ThenInclude(bu => bu.User).ThenInclude(u => u.Address)
                .Where(x => x.Id == bookId)
                .FirstOrDefaultAsync();
        }

        public async Task RenewChooseDateAsync(Guid bookId)
        {
            var book = await _repository.FindAsync(bookId);
            if (book == null)
                throw new ShareBookException(ShareBookException.Error.NotFound);

            if (!book.MayChooseWinner())
                throw new ShareBookException(ShareBookException.Error.BadRequest, "Aguarde a data de decisão.");

            book.Status = BookStatus.Available;
            book.ChooseDate = DateTime.UtcNow.AddDays(10);
            await _repository.UpdateAsync(book);
        }

        #region Private

        private async Task<PagedList<Book>> SearchBooksAsync(Expression<Func<Book, bool>> filter, int page, int itemsPerPage)
            => await SearchBooksAsync(filter, page, itemsPerPage, x => x.CreationDate);

        private async Task<PagedList<Book>> SearchBooksAsync<TKey>(Expression<Func<Book, bool>> filter, int page, int itemsPerPage, Expression<Func<Book, TKey>> expression)
        {
            var query = _repository.Get()
                .Where(filter)
                .OrderByDescending(expression)
                .Select(u => new Book
                {
                    Id = u.Id,
                    Title = u.Title,
                    Author = u.Author,
                    Status = u.Status,
                    FreightOption = u.FreightOption,
                    ImageUrl = _uploadService.GetImageUrl(u.ImageSlug, "Books"),
                    Slug = u.Slug,
                    CreationDate = u.CreationDate,
                    Synopsis = u.Synopsis,
                    ChooseDate = u.ChooseDate,
                    User = new User()
                    {
                        Id = u.User.Id,
                        Email = u.User.Email,
                        Name = u.User.Name,
                        Linkedin = u.User.Linkedin,
                        Address = new Address()
                        {
                            City = u.User.Address.City,
                            State = u.User.Address.State,
                            Country = u.User.Address.Country,
                            UserId = u.User.Address.UserId,
                            Id = u.User.Address.Id,
                            CreationDate = u.User.Address.CreationDate,
                        }
                    },
                    CategoryId = u.CategoryId,
                    Category = u.Category
                });

            return await FormatPagedListAsync(query, page, itemsPerPage);
        }

        private string SetSlugByTitleOrIncremental(Book entity)
        {
            // TODO: Migrate to async/await (P.s: breaking unit tests)
            var slug = _repository.Get()
                        .Where(x => x.Title.ToUpper().Trim().Equals(entity.Title.ToUpper().Trim())
                                    && !x.Id.Equals(entity.Id))
                        .OrderByDescending(x => x.CreationDate).FirstOrDefault()?.Slug;

            return string.IsNullOrWhiteSpace(slug) ? entity.Title.GenerateSlug() : slug.AddIncremental();
        }

        private void EBookValidate(Book entity)
        {
            if (entity.Type == BookType.Eletronic &&
                string.IsNullOrEmpty(entity.EBookDownloadLink) &&
                string.IsNullOrEmpty(entity.EBookPdfFile))
            {
                throw new ShareBookException(ShareBookException.Error.BadRequest,
                    "Necessário informar o link ou o arquivo em caso de um E-Book.");
            }
        }

        public async Task<BookStatsDTO> GetStatsAsync()
        {
            var groupedStatus = await _repository.Get()
                .GroupBy(b => b.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Total = g.Count()
                })
                .ToListAsync();

            var status = new BookStatsDTO();

            status.TotalWaitingApproval = groupedStatus.Exists(g => g.Status == BookStatus.WaitingApproval)
                ? groupedStatus.Find(g => g.Status == BookStatus.WaitingApproval).Total
                : 0;

            status.TotalOk = groupedStatus
                .Where(g => g.Status == BookStatus.WaitingSend || g.Status == BookStatus.Sent || g.Status == BookStatus.Received)
                .Sum(g => g.Total);

            return status;
        }

        #endregion Private
    }
}