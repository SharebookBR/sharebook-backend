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
using ShareBook.Service.EBook;
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
        private readonly IEBookService _ebookService;

        private readonly NewBookQueue _newBookQueue;

        public BookService(IBookRepository bookRepository,
                    IUnitOfWork unitOfWork, IValidator<Book> validator,
                    IUploadService uploadService, IBooksEmailService booksEmailService, IConfiguration configuration,
                    NewBookQueue newBookQueue, IEBookService ebookService)
                    : base(bookRepository, unitOfWork, validator)
        {
            _uploadService = uploadService;
            _booksEmailService = booksEmailService;
            _configuration = configuration;
            _newBookQueue = newBookQueue;
            _ebookService = ebookService;
        }

        public async Task ApproveAsync(Guid bookId, DateTime? chooseDate = null)
        {
            var daysInShowcase = int.Parse(_configuration["SharebookSettings:DaysInShowcase"]);

            var book = await _repository.Get().Include(b => b.Category).FirstOrDefaultAsync(b => b.Id == bookId);
            if (book == null)
                throw new ShareBookException(ShareBookException.Error.NotFound);

            book.Status = BookStatus.Available;
            book.ApprovedAt = DateTime.UtcNow;
            book.ChooseDate = book.IsEbook()
                ? null
                : chooseDate?.Date ?? DateTime.Today.AddDays(daysInShowcase);
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

        public async Task MarkAsDeliveredAsync(Guid bookId)
        {
            var book = await _repository.FindAsync(bookId);
            if (book == null)
                throw new ShareBookException(ShareBookException.Error.NotFound);

            book.Status = BookStatus.Received;
            await _repository.UpdateAsync(book);
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
                    .ThenInclude(c => c.ParentCategory)
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
                    .ThenInclude(c => c.ParentCategory)
                    .Where(b => b.Status == BookStatus.Available)
                    .OrderBy(x => Guid.NewGuid()) // ordem aleatória
                    .Take(15) // apenas 15 registros
                    .ToListAsync()
             );
        }

        public async Task<IList<Book>> GetNewest15EBooksAsync()
        {
            return SetImageUrl(
                await _repository.Get()
                    .Include(b => b.User)
                    .ThenInclude(u => u.Address)
                    .Include(b => b.Category)
                    .ThenInclude(c => c.ParentCategory)
                    .Where(b => b.Status == BookStatus.Available && b.Type == BookType.Eletronic)
                    .OrderByDescending(x => x.CreationDate) // os mais novos primeiro
                    .Take(15) // apenas 15 registros
                    .ToListAsync()
             );
        }

        public async Task<PagedList<Book>> RecentEBooksAsync(int page, int itemsPerPage, int days = 7)
        {
            var since = DateTime.UtcNow.AddDays(-days);

            return await SearchBooksAsync(
                x => x.Status == BookStatus.Available
                    && x.Type == BookType.Eletronic
                    && x.ApprovedAt.HasValue
                    && x.ApprovedAt.Value >= since,
                page,
                itemsPerPage,
                x => x.ApprovedAt ?? x.CreationDate);
        }

        public async Task<int> GetAvailableEBooksCountAsync()
        {
            return await _repository.Get()
                .Where(b => b.Status == BookStatus.Available && b.Type == BookType.Eletronic)
                .CountAsync();
        }

        public async Task<AdminBooksResultDTO> GetAdminBooksAsync(
            int page,
            int itemsPerPage,
            string search = null,
            string status = null,
            string bucket = null,
            string type = null)
        {
            var normalizedPage = page <= 0 ? 1 : page;
            var normalizedItemsPerPage = itemsPerPage <= 0 ? 24 : Math.Min(itemsPerPage, 100);

            var baseQuery = BuildAdminBooksQuery();
            var summary = await BuildAdminSummaryAsync(baseQuery);

            var filteredQuery = ApplyAdminBooksFilters(baseQuery, search, status, bucket, type);
            var totalItems = await filteredQuery.CountAsync();
            var totalPages = Math.Max((int)Math.Ceiling(totalItems / (double)normalizedItemsPerPage), 1);
            var effectivePage = Math.Min(normalizedPage, totalPages);
            var skip = (effectivePage - 1) * normalizedItemsPerPage;

            var books = await filteredQuery
                .OrderByDescending(x => x.CreationDate)
                .ThenByDescending(x => x.Id)
                .Skip(skip)
                .Take(normalizedItemsPerPage)
                .ToListAsync();

            return new AdminBooksResultDTO
            {
                Page = effectivePage,
                ItemsPerPage = normalizedItemsPerPage,
                TotalItems = totalItems,
                Summary = summary,
                Items = SetImageUrl(books)
            };
        }

        private IList<Book> SetImageUrl(IList<Book> books)
        {
            return books.Select(b => { b.ImageUrl = _uploadService.GetImageUrl(b.ImageSlug, "Books"); return b; }).ToList();
        }

        private IQueryable<Book> BuildAdminBooksQuery()
        {
            return _repository.Get()
                .Include(b => b.User)
                    .ThenInclude(u => u.Address)
                .Include(b => b.BookUsers)
                    .ThenInclude(bu => bu.User)
                .Include(b => b.UserFacilitator)
                .Include(b => b.Category)
                    .ThenInclude(c => c.ParentCategory);
        }

        private async Task<AdminBooksSummaryDTO> BuildAdminSummaryAsync(IQueryable<Book> query)
        {
            var totalItems = await query.CountAsync();
            var statusCounts = await query
                .GroupBy(x => x.Status)
                .Select(x => new { Status = x.Key, Count = x.Count() })
                .ToListAsync();
            var typeCounts = await query
                .GroupBy(x => x.Type)
                .Select(x => new { Type = x.Key, Count = x.Count() })
                .ToListAsync();

            int GetStatusCount(BookStatus status) => statusCounts.FirstOrDefault(x => x.Status == status)?.Count ?? 0;
            int GetTypeCount(BookType bookType) => typeCounts.FirstOrDefault(x => x.Type == bookType)?.Count ?? 0;

            return new AdminBooksSummaryDTO
            {
                All = totalItems,
                NeedsAction = GetStatusCount(BookStatus.WaitingApproval) + GetStatusCount(BookStatus.AwaitingDonorDecision),
                Shipping = GetStatusCount(BookStatus.WaitingSend) + GetStatusCount(BookStatus.Sent),
                Physical = GetTypeCount(BookType.Printed),
                Ebooks = GetTypeCount(BookType.Eletronic),
                Finished = GetStatusCount(BookStatus.Received) + GetStatusCount(BookStatus.Canceled),
                Available = GetStatusCount(BookStatus.Available)
            };
        }

        private IQueryable<Book> ApplyAdminBooksFilters(
            IQueryable<Book> query,
            string search,
            string status,
            string bucket,
            string type)
        {
            var normalizedSearch = (search ?? string.Empty).Trim().ToLower();
            if (!string.IsNullOrWhiteSpace(normalizedSearch))
            {
                query = query.Where(x =>
                    (x.Title != null && x.Title.ToLower().Contains(normalizedSearch))
                    || (x.Author != null && x.Author.ToLower().Contains(normalizedSearch))
                    || (x.User != null && x.User.Name != null && x.User.Name.ToLower().Contains(normalizedSearch))
                    || (x.UserFacilitator != null && x.UserFacilitator.Name != null && x.UserFacilitator.Name.ToLower().Contains(normalizedSearch))
                    || x.BookUsers.Any(bu =>
                        bu.Status == DonationStatus.Donated
                        && bu.User != null
                        && bu.User.Name != null
                        && bu.User.Name.ToLower().Contains(normalizedSearch))
                );
            }

            if (TryParseBookStatus(status, out var parsedStatus))
                query = query.Where(x => x.Status == parsedStatus);

            if (TryParseBookType(type, out var parsedType))
                query = query.Where(x => x.Type == parsedType);

            if (!string.IsNullOrWhiteSpace(bucket))
                query = ApplyAdminBucket(query, bucket);

            return query;
        }

        private IQueryable<Book> ApplyAdminBucket(IQueryable<Book> query, string bucket)
        {
            switch ((bucket ?? string.Empty).Trim().ToLower())
            {
                case "needsaction":
                    return query.Where(x => x.Status == BookStatus.WaitingApproval || x.Status == BookStatus.AwaitingDonorDecision);
                case "shipping":
                    return query.Where(x => x.Status == BookStatus.WaitingSend || x.Status == BookStatus.Sent);
                case "finished":
                    return query.Where(x => x.Status == BookStatus.Received || x.Status == BookStatus.Canceled);
                case "ebooks":
                    return query.Where(x => x.Type == BookType.Eletronic);
                case "physical":
                    return query.Where(x => x.Type == BookType.Printed);
                case "available":
                    return query.Where(x => x.Status == BookStatus.Available);
                default:
                    return query;
            }
        }

        private bool TryParseBookStatus(string status, out BookStatus parsedStatus)
        {
            parsedStatus = default;
            return !string.IsNullOrWhiteSpace(status)
                && Enum.TryParse(status.Trim(), true, out parsedStatus);
        }

        private bool TryParseBookType(string type, out BookType parsedType)
        {
            parsedType = default;

            switch ((type ?? string.Empty).Trim().ToLower())
            {
                case "printed":
                case "physical":
                    parsedType = BookType.Printed;
                    return true;
                case "eletronic":
                case "ebook":
                case "ebooks":
                case "digital":
                    parsedType = BookType.Eletronic;
                    return true;
                default:
                    return false;
            }
        }

        private async Task<UserDonationsSummaryDTO> BuildUserDonationsSummaryAsync(IQueryable<Book> query)
        {
            var statusCounts = await query
                .GroupBy(x => x.Status)
                .Select(x => new { Status = x.Key, Count = x.Count() })
                .ToListAsync();
            var ebookDownloadsTotal = await query
                .Where(x => x.Type == BookType.Eletronic)
                .SumAsync(x => (int?)x.DownloadCount) ?? 0;

            int GetStatusCount(BookStatus status) => statusCounts.FirstOrDefault(x => x.Status == status)?.Count ?? 0;

            return new UserDonationsSummaryDTO
            {
                WaitingDecision = GetStatusCount(BookStatus.AwaitingDonorDecision),
                WaitingSend = GetStatusCount(BookStatus.WaitingSend),
                Finished = GetStatusCount(BookStatus.Received) + GetStatusCount(BookStatus.Canceled),
                EbookDownloadsTotal = ebookDownloadsTotal
            };
        }

        private IQueryable<Book> ApplyUserDonationsFilters(IQueryable<Book> query, string search, string bucket)
        {
            if (!string.IsNullOrWhiteSpace(bucket))
            {
                switch (bucket.Trim().ToLower())
                {
                    case "needsaction":
                        query = query.Where(x => x.Status == BookStatus.AwaitingDonorDecision || x.Status == BookStatus.WaitingSend);
                        break;
                    case "physical":
                        query = query.Where(x => x.Type == BookType.Printed);
                        break;
                    case "digital":
                        query = query.Where(x => x.Type == BookType.Eletronic);
                        break;
                    case "finished":
                        query = query.Where(x => x.Status == BookStatus.Received || x.Status == BookStatus.Canceled);
                        break;
                }
            }

            var normalizedSearch = (search ?? string.Empty).Trim().ToLower();
            if (!string.IsNullOrWhiteSpace(normalizedSearch))
            {
                query = query.Where(x =>
                    (x.Title != null && x.Title.ToLower().Contains(normalizedSearch))
                    || (x.Author != null && x.Author.ToLower().Contains(normalizedSearch))
                    || x.Status.ToString().ToLower().Contains(normalizedSearch)
                    || (normalizedSearch.Contains("digital") && x.Type == BookType.Eletronic)
                    || (normalizedSearch.Contains("fisico") && x.Type == BookType.Printed)
                    || (normalizedSearch.Contains("físico") && x.Type == BookType.Printed)
                );
            }

            return query;
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

            if (entity.IsEbook())
                entity.ChooseDate = null;

            var result = await ValidateAsync(entity);
            if (result.Success && entity.IsEbook())
            {
                var isDuplicate = await _repository.AnyAsync(b =>
                    b.Type == BookType.Eletronic &&
                    b.Title.ToLower().Trim() == entity.Title.ToLower().Trim() &&
                    b.Author.ToLower().Trim() == entity.Author.ToLower().Trim());

                if (isDuplicate)
                {
                    result.Messages.Add("Já existe um e-book com este título e autor no catálogo.");
                    return result;
                }
            }

            if (result.Success)
            {
                entity.Slug = SetSlugByTitleOrIncremental(entity);

                entity.ImageSlug = ImageHelper.FormatImageName(entity.ImageName, entity.Slug);

                if (entity.HasPdfToUpload())
                    entity.EBookPdfPath = await _ebookService.UploadPdfAsync(entity);

                result.Value = await _repository.InsertAsync(entity);

                result.Value.ImageUrl = await _uploadService.UploadImageAsync(entity.ImageBytes, entity.ImageSlug, "Books");

                result.Value.ImageBytes = null;
                result.Value.PdfBytes = null;

                await _booksEmailService.SendEmailNewBookInsertedAsync(entity);
            }
            return result;
        }

        public override async Task<Result<Book>> UpdateAsync(Book entity)
        {
            Result<Book> result = Validate(entity, x =>
                x.Title,
                x => x.Author,
                x => x.Id);

            var bookId = entity.Id;

            if (!result.Success) return result;

            //buscar o book no banco para obter um objeto para ser re-hidratado
            var savedBook = await this._repository.FindAsync(bookId);

            if (savedBook == null)
                throw new ShareBookException(ShareBookException.Error.NotFound);

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
            savedBook.ChooseDate = savedBook.IsEbook()
                ? null
                : entity.ChooseDate?.Date;

            if (entity.UserIdFacilitator.HasValue && entity.UserIdFacilitator != Guid.Empty)
                savedBook.UserIdFacilitator = entity.UserIdFacilitator;

            result.Value = await _repository.UpdateAsync(savedBook);
            result.Value.ImageBytes = null;

            return result;
        }

        public async Task<PagedList<Book>> FullSearchAsync(string criteria, int page, int itemsPerPage, bool isAdmin)
        {
            criteria = (criteria ?? string.Empty).Trim().ToLower();

            Expression<Func<Book, bool>> filter = x =>
                (x.Author.ToLower().Contains(criteria)
                 || x.Title.ToLower().Contains(criteria)
                 || x.Category.Name.ToLower().Contains(criteria))
                && x.Status == BookStatus.Available;

            if (!isAdmin)
            {
                filter = x =>
                    x.Author.ToLower().Contains(criteria)
                    || x.Title.ToLower().Contains(criteria)
                    || x.Category.Name.ToLower().Contains(criteria);
            }

            return await SearchBooksAsync(filter, page, itemsPerPage);
        }

        public async Task<PagedList<Book>> ByCategoryIdAsync(Guid categoryId, int page, int itemsPerPage)
            => await SearchBooksAsync(x => x.Status == BookStatus.Available && x.CategoryId == categoryId, page, itemsPerPage);

        public async Task<PagedList<Book>> ByCategoryTreeIdAsync(Guid categoryId, int page, int itemsPerPage)
            => await SearchBooksAsync(
                x => x.Status == BookStatus.Available
                    && (x.CategoryId == categoryId || x.Category.ParentCategoryId == categoryId),
                page,
                itemsPerPage);

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
                    x.BookUsers.Any(y => y.UserId == userId));
        }

        public override async Task<PagedList<Book>> GetAsync<TKey>(
            Expression<Func<Book, bool>> filter,
            Expression<Func<Book, TKey>> order,
            int page,
            int itemsPerPage,
            bool descending = false)
            => await base.GetAsync(filter, order, page, itemsPerPage, descending);

        public async Task<IList<Book>> GetUserDonationsAsync(Guid userId)
        {
            return await _repository.Get()
                .Include(b => b.BookUsers)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreationDate)
                .ToListAsync();
        }

        public async Task<UserDonationsResultDTO> GetUserDonationsAsync(
            Guid userId,
            int page,
            int itemsPerPage,
            string search = null,
            string bucket = null)
        {
            var normalizedPage = page <= 0 ? 1 : page;
            var normalizedItemsPerPage = itemsPerPage <= 0 ? 24 : Math.Min(itemsPerPage, 100);

            var baseQuery = _repository.Get()
                .Include(b => b.BookUsers)
                    .ThenInclude(bu => bu.User)
                .Include(b => b.Category)
                    .ThenInclude(c => c.ParentCategory)
                .Where(b => b.UserId == userId);

            var summary = await BuildUserDonationsSummaryAsync(baseQuery);
            var filteredQuery = ApplyUserDonationsFilters(baseQuery, search, bucket);
            var totalItems = await filteredQuery.CountAsync();
            var totalPages = Math.Max((int)Math.Ceiling(totalItems / (double)normalizedItemsPerPage), 1);
            var effectivePage = Math.Min(normalizedPage, totalPages);
            var skip = (effectivePage - 1) * normalizedItemsPerPage;

            var items = await filteredQuery
                .OrderByDescending(x => x.CreationDate)
                .ThenByDescending(x => x.Id)
                .Skip(skip)
                .Take(normalizedItemsPerPage)
                .ToListAsync();

            return new UserDonationsResultDTO
            {
                Page = effectivePage,
                ItemsPerPage = normalizedItemsPerPage,
                TotalItems = totalItems,
                Summary = summary,
                Items = SetImageUrl(items)
            };
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

        public async Task ReportCopyrightAsync(string slug)
        {
            var book = await BySlugAsync(slug);
            if (book == null)
                throw new ShareBookException(ShareBookException.Error.NotFound);

            if (!book.IsEbook())
                throw new ShareBookException(ShareBookException.Error.BadRequest, "Report de direitos autorais é aplicável apenas a e-books.");

            await _booksEmailService.SendEmailCopyrightReportAsync(book);
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
                    Category = new Category()
                    {
                        Id = u.Category.Id,
                        Name = u.Category.Name,
                        ParentCategoryId = u.Category.ParentCategoryId,
                        ParentCategory = u.Category.ParentCategory == null
                            ? null
                            : new Category()
                            {
                                Id = u.Category.ParentCategory.Id,
                                Name = u.Category.ParentCategory.Name
                            }
                    },
                    Type = u.Type,
                    EBookPdfPath = u.EBookPdfPath
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

        public async Task IncrementDownloadCountAsync(Guid bookId)
        {
            var book = await _repository.FindAsync(bookId);
            if (book == null)
                throw new ShareBookException(ShareBookException.Error.NotFound);

            book.DownloadCount++;
            await _repository.UpdateAsync(book);
        }

        #endregion Private
    }
}
