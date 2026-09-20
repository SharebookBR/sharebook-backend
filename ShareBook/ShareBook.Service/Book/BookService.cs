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

namespace ShareBook.Service;

public partial class BookService : BaseService<Book>, IBookService
{
    private const int MaxSlugInsertAttempts = 5;
    private readonly IUploadService _uploadService;
    private readonly IBookRepository _bookRepository;
    private readonly IBooksEmailService _booksEmailService;
    private readonly IConfiguration _configuration;
    private readonly IEBookService _ebookService;
    private readonly ICategoryRepository _categoryRepository;

    private readonly NewBookQueue _newBookQueue;

    public BookService(IBookRepository bookRepository,
                IUnitOfWork unitOfWork, IValidator<Book> validator,
                IUploadService uploadService, IBooksEmailService booksEmailService, IConfiguration configuration,
                NewBookQueue newBookQueue, IEBookService ebookService, ICategoryRepository categoryRepository)
                : base(bookRepository, unitOfWork, validator)
    {
        _bookRepository = bookRepository;
        _uploadService = uploadService;
        _booksEmailService = booksEmailService;
        _configuration = configuration;
        _newBookQueue = newBookQueue;
        _ebookService = ebookService;
        _categoryRepository = categoryRepository;
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

    private IList<Book> SetImageUrls(IList<Book> books)
    {
        return books.Select(book =>
        {
            SetImageUrls(book);
            return book;
        }).ToList();
    }

    private void SetImageUrls(Book book)
    {
        book.ImageUrl = _uploadService.GetImageUrl(book.ImageSlug, "Books", book.ImageVersion);
        book.ThumbnailUrl = _uploadService.GetBookThumbnailUrl(book.ImageSlug, book.ImageVersion);
    }

    private bool IsLeafCategory(Guid categoryId)
    {
        var hasChildren = _categoryRepository
            .Get()
            .Any(c => c.ParentCategoryId == categoryId);

        return !hasChildren;
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

        SetImageUrls(result);

        return result;
    }

    public override async Task<Result<Book>> InsertAsync(Book entity)
    {
        entity.UserId = new Guid(Thread.CurrentPrincipal?.Identity?.Name);

        if (entity.IsEbook())
            entity.ChooseDate = null;

        var result = await ValidateAsync(entity);
        if (result.Success)
        {
            var isLeafCategory = IsLeafCategory(entity.CategoryId);
            if (!isLeafCategory)
            {
                result.Messages.Add("Selecione uma subcategoria final (categoria folha). Categorias-pai não são permitidas para cadastro.");
                return result;
            }
        }

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
            for (var attempt = 1; attempt <= MaxSlugInsertAttempts; attempt++)
            {
                entity.Slug = await GetAvailableSlugAsync(entity.Title);
                entity.ImageSlug = ImageHelper.FormatImageName(entity.ImageName, entity.Slug);

                var uploadedPdf = entity.HasPdfToUpload();
                if (uploadedPdf)
                    entity.EBookPdfPath = await _ebookService.UploadPdfAsync(entity);

                try
                {
                    result.Value = await _repository.InsertAsync(entity);
                    break;
                }
                catch (DuplicateBookSlugException) when (attempt < MaxSlugInsertAttempts)
                {
                    await DeleteUploadedPdfAfterSlugConflictAsync(entity, uploadedPdf);
                }
                catch (DuplicateBookSlugException)
                {
                    await DeleteUploadedPdfAfterSlugConflictAsync(entity, uploadedPdf);
                    throw;
                }
            }

            await _uploadService.UploadImageAsync(entity.ImageBytes, entity.ImageSlug, "Books");
            SetImageUrls(result.Value);

            result.Value.ImageBytes = null;
            result.Value.PdfBytes = null;

            await _booksEmailService.SendEmailNewBookInsertedAsync(entity);
        }
        return result;
    }

    public override async Task<Result> DeleteAsync(params object[] keyValues)
    {
        var book = await _repository.FindAsync(keyValues);
        if (book == null)
            throw new ShareBookException(ShareBookException.Error.NotFound);

        if (!string.IsNullOrWhiteSpace(book.ImageSlug))
        {
            try
            {
                await _uploadService.DeleteFileIfExistsAsync(book.ImageSlug, "Books");
            }
            catch
            {
                // limpeza de arquivo não deve bloquear remoção do registro no banco
            }
        }

        if (book.IsEbook() && !string.IsNullOrWhiteSpace(book.EBookPdfPath))
        {
            try
            {
                await _ebookService.DeletePdfAsync(book);
            }
            catch
            {
                // limpeza de arquivo não deve bloquear remoção do registro no banco
            }
        }

        return await base.DeleteAsync(keyValues);
    }

    public override async Task<Result<Book>> UpdateAsync(Book entity)
    {
        Result<Book> result = Validate(entity, x =>
            x.Title,
            x => x.Author,
            x => x.Id);

        var bookId = entity.Id;

        if (!result.Success) return result;

        var isLeafCategory = IsLeafCategory(entity.CategoryId);
        if (!isLeafCategory)
        {
            result.Messages.Add("Selecione uma subcategoria final (categoria folha). Categorias-pai não são permitidas para cadastro.");
            return result;
        }

        //buscar o book no banco para obter um objeto para ser re-hidratado
        var savedBook = await this._repository.FindAsync(bookId);

        if (savedBook == null)
            throw new ShareBookException(ShareBookException.Error.NotFound);

        //imagem eh opcional no update
        if (!string.IsNullOrEmpty(entity.ImageName) && entity.ImageBytes.Length > 0)
        {
            entity.ImageSlug = ImageHelper.FormatImageName(entity.ImageName, savedBook.Slug);

            await _uploadService.UploadImageAsync(entity.ImageBytes, entity.ImageSlug, "Books");

            if (!string.IsNullOrWhiteSpace(savedBook.ImageSlug)
                && !savedBook.ImageSlug.Equals(entity.ImageSlug, StringComparison.OrdinalIgnoreCase))
            {
                await _uploadService.DeleteReplacedImageAsync(
                    savedBook.ImageSlug,
                    entity.ImageSlug,
                    "Books");
            }

            savedBook.ImageVersion++;
        }

        //preparar o book para atualização
        savedBook.Author = entity.Author;
        savedBook.FreightOption = entity.FreightOption;
        savedBook.Author = entity.Author;

        // Imagem é opcional no update e o UpdateBookVM não carrega ImageSlug.
        // Copiar cegamente apagava a capa de quem só quis editar texto.
        if (!string.IsNullOrWhiteSpace(entity.ImageSlug))
            savedBook.ImageSlug = entity.ImageSlug;

        savedBook.Title = entity.Title;
        savedBook.CategoryId = entity.CategoryId;

        savedBook.Synopsis = entity.Synopsis;
        savedBook.TrackingNumber = entity.TrackingNumber;
        savedBook.ChooseDate = savedBook.IsEbook()
            ? null
            : entity.ChooseDate?.Date;

        if (entity.UserIdFacilitator.HasValue && entity.UserIdFacilitator != Guid.Empty)
            savedBook.UserIdFacilitator = entity.UserIdFacilitator;

        result.Value = await _repository.UpdateAsync(savedBook);
        result.Value.ImageBytes = null;
        SetImageUrls(result.Value);

        return result;
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

    public async Task IncrementDownloadCountAsync(Guid bookId)
    {
        var book = await _repository.FindAsync(bookId);
        if (book == null)
            throw new ShareBookException(ShareBookException.Error.NotFound);

        book.DownloadCount++;
        await _repository.UpdateAsync(book);
    }

    #region Private

    private async Task<string> GetAvailableSlugAsync(string title)
    {
        var baseSlug = title.GenerateSlug();
        var existingSlugs = await _bookRepository.GetSlugsStartingWithAsync(baseSlug);

        return baseSlug.NextAvailableCopySlug(existingSlugs);
    }

    private async Task DeleteUploadedPdfAfterSlugConflictAsync(Book entity, bool uploadedPdf)
    {
        if (!uploadedPdf || string.IsNullOrWhiteSpace(entity.EBookPdfPath))
            return;

        try
        {
            await _ebookService.DeletePdfAsync(entity);
        }
        catch
        {
            // A disputa de slug não deve impedir a nova tentativa de persistência.
        }
        finally
        {
            entity.EBookPdfPath = null;
        }
    }

    #endregion Private
}
