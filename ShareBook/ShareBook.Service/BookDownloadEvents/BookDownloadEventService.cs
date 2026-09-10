using Microsoft.EntityFrameworkCore;
using ShareBook.Domain;
using ShareBook.Domain.DTOs;
using ShareBook.Domain.Enums;
using ShareBook.Repository;
using ShareBook.Service.Upload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShareBook.Service.BookDownloadEvents
{
    public class BookDownloadEventService : IBookDownloadEventService
    {
        private readonly IBookDownloadEventRepository _downloadEventRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IUploadService _uploadService;

        public BookDownloadEventService(
            IBookDownloadEventRepository downloadEventRepository,
            IBookRepository bookRepository,
            IUploadService uploadService)
        {
            _downloadEventRepository = downloadEventRepository;
            _bookRepository = bookRepository;
            _uploadService = uploadService;
        }

        public async Task RecordAsync(Guid bookId, Guid? userId, BookDownloadEventSource source)
        {
            await _downloadEventRepository.InsertAsync(new BookDownloadEvent
            {
                BookId = bookId,
                UserId = userId,
                Source = source,
                DownloadedAtUtc = DateTime.UtcNow
            });
        }

        public async Task<List<HomeShowcaseBookDTO>> GetTopDownloadedEbooksAsync(int days, int limit)
        {
            var windowDays = days <= 0 ? 30 : days;
            var take = limit <= 0 ? 15 : limit;
            var since = DateTime.UtcNow.AddDays(-windowDays);

            var rankedDownloads = await _downloadEventRepository.Get()
                .AsNoTracking()
                .Where(download => download.DownloadedAtUtc >= since)
                .GroupBy(download => download.BookId)
                .Select(downloadsByBook => new
                {
                    BookId = downloadsByBook.Key,
                    Count = downloadsByBook.Count(),
                    LastDownloadedAtUtc = downloadsByBook.Max(download => download.DownloadedAtUtc)
                })
                .OrderByDescending(download => download.Count)
                .ThenByDescending(download => download.LastDownloadedAtUtc)
                .Take(take)
                .ToListAsync();

            var rankedBookIds = rankedDownloads.Select(download => download.BookId).ToList();
            if (!rankedBookIds.Any())
                return new List<HomeShowcaseBookDTO>();

            var booksById = await _bookRepository.Get()
                .AsNoTracking()
                .Where(book => rankedBookIds.Contains(book.Id)
                            && book.Status == BookStatus.Available
                            && book.Type == BookType.Eletronic)
                .ToDictionaryAsync(book => book.Id);

            return rankedDownloads
                .Where(download => booksById.ContainsKey(download.BookId))
                .Select(download => ToShowcaseBook(booksById[download.BookId]))
                .ToList();
        }

        private HomeShowcaseBookDTO ToShowcaseBook(Book book)
        {
            return new HomeShowcaseBookDTO
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Slug = book.Slug,
                ImageUrl = _uploadService.GetImageUrl(book.ImageSlug, "Books", book.ImageVersion),
                ThumbnailUrl = _uploadService.GetBookThumbnailUrl(book.ImageSlug, book.ImageVersion),
                Type = book.Type.ToString()
            };
        }
    }
}
