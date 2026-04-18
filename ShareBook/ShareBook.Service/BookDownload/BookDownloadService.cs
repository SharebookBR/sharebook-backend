using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ShareBook.Domain;
using ShareBook.Domain.Exceptions;
using ShareBook.Repository;
using ShareBook.Repository.Repository;
using ShareBook.Repository.UoW;
using ShareBook.Service.Generic;
using System;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public class BookDownloadService : BaseService<BookDownload>, IBookDownloadService
    {
        private readonly IBookDownloadRepository _bookDownloadRepository;
        private readonly IBookService _bookService;
        private readonly ILogger<BookDownloadService> _logger;

        public BookDownloadService(
            IBookDownloadRepository bookDownloadRepository,
            IBookService bookService,
            IUnitOfWork unitOfWork,
            IValidator<BookDownload> validator,
            ILogger<BookDownloadService> logger = null)
            : base(bookDownloadRepository, unitOfWork, validator)
        {
            _bookDownloadRepository = bookDownloadRepository;
            _bookService = bookService;
            _logger = logger ?? NullLogger<BookDownloadService>.Instance;
        }

        public async Task RegisterDownloadAsync(Guid bookId, Guid? userId, string userAgent, string ipAddress)
        {
            // Verificar se o livro existe
            var book = await _bookService.FindAsync(bookId);
            if (book == null)
                throw new ShareBookException(ShareBookException.Error.NotFound);

            var download = new BookDownload
            {
                BookId = bookId,
                UserId = userId,
                UserAgent = userAgent,
                IpAddress = ipAddress,
                DownloadedAt = DateTime.UtcNow
            };

            await _bookDownloadRepository.InsertAsync(download);

            // Incrementar contador agregado no Book (opcional, já existe IncrementDownloadCountAsync)
            await _bookService.IncrementDownloadCountAsync(bookId);

            _logger.LogInformation("Download registrado: BookId={BookId}, UserId={UserId}", bookId, userId);
        }

        public async Task<int> GetDownloadCountAsync(Guid bookId)
        {
            return await _bookDownloadRepository.CountAsync(bd => bd.BookId == bookId);
        }
    }
}