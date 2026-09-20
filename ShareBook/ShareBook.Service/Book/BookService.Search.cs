using Microsoft.EntityFrameworkCore;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Domain.DTOs;
using ShareBook.Domain.Enums;
using ShareBook.Helper.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ShareBook.Service;

public partial class BookService
{
    public async Task<IList<Book>> Random15BooksAsync()
    {
        return SetImageUrls(
            await _repository.Get()
                .Include(b => b.User)
                .ThenInclude(u => u!.Address)
                .Include(b => b.Category)
                .ThenInclude(c => c.ParentCategory)
                .Where(b => b.Status == BookStatus.Available)
                .OrderBy(x => EF.Functions.Random()) // ordem aleatória
                .Take(15) // apenas 15 registros
                .ToListAsync()
         );
    }

    public async Task<IList<Book>> GetNewest15EBooksAsync()
    {
        return SetImageUrls(
            await _repository.Get()
                .Include(b => b.User)
                .ThenInclude(u => u!.Address)
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

    public async Task<int> GetRecentEBooksCountAsync(int days = 7)
    {
        var since = DateTime.UtcNow.AddDays(-days);

        return await _repository.Get()
            .Where(b => b.Status == BookStatus.Available
                && b.Type == BookType.Eletronic
                && b.ApprovedAt.HasValue
                && b.ApprovedAt.Value >= since)
            .CountAsync();
    }

    public async Task<IList<SitemapBookDTO>> GetSitemapBooksAsync()
    {
        return await _repository.Get()
            .AsNoTracking()
            .Where(b => b.Status != BookStatus.WaitingApproval
                && b.Status != BookStatus.Canceled
                && !string.IsNullOrWhiteSpace(b.Slug))
            .OrderBy(b => b.Slug)
            .Select(b => new SitemapBookDTO
            {
                Slug = b.Slug ?? string.Empty,
                LastModifiedAt = b.ApprovedAt ?? b.CreationDate
            })
            .ToListAsync();
    }

    public async Task<PagedList<Book>> FullSearchAsync(string criteria, int page, int itemsPerPage, bool isAdmin)
    {
        var normalizedCriteria = criteria.ToNormalizedSearchText();
        var query = _bookRepository
            .FullTextSearch(normalizedCriteria, includeUnavailable: isAdmin)
            .Select(book => new Book
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Status = book.Status,
                DownloadCount = book.DownloadCount,
                FreightOption = book.FreightOption,
                ImageSlug = book.ImageSlug,
                ImageVersion = book.ImageVersion,
                ImageUrl = _uploadService.GetImageUrl(book.ImageSlug ?? string.Empty, "Books", book.ImageVersion),
                ThumbnailUrl = _uploadService.GetBookThumbnailUrl(book.ImageSlug ?? string.Empty, book.ImageVersion),
                Slug = book.Slug,
                CreationDate = book.CreationDate,
                Synopsis = book.Synopsis,
                ChooseDate = book.ChooseDate,
                User = new User
                {
                    Id = book.User!.Id,
                    Email = book.User.Email,
                    Name = book.User.Name,
                    Linkedin = book.User.Linkedin,
                    Address = new Address
                    {
                        City = book.User.Address!.City,
                        State = book.User.Address.State,
                        Country = book.User.Address.Country,
                        UserId = book.User.Address.UserId,
                        Id = book.User.Address.Id,
                        CreationDate = book.User.Address.CreationDate,
                    }
                },
                CategoryId = book.CategoryId,
                Category = new Category
                {
                    Id = book.Category.Id,
                    Name = book.Category.Name,
                    ParentCategoryId = book.Category.ParentCategoryId,
                    ParentCategory = book.Category.ParentCategory == null
                        ? null
                        : new Category
                        {
                            Id = book.Category.ParentCategory.Id,
                            Name = book.Category.ParentCategory.Name
                        }
                },
                Type = book.Type,
                EBookPdfPath = book.EBookPdfPath
            });

        return await FormatPagedListAsync(query, page, itemsPerPage);
    }

    public async Task<CategoryBooksResultDTO> ByCategoryIdAsync(Guid categoryId, int page, int itemsPerPage)
    {
        var query = _repository.Get()
            .Where(x => x.Status == BookStatus.Available && x.CategoryId == categoryId);

        return await FormatCategoryBooksResultAsync(query, page, itemsPerPage);
    }

    public async Task<CategoryBooksResultDTO> ByCategoryTreeIdAsync(Guid categoryId, int page, int itemsPerPage)
    {
        var query = _repository.Get()
            .Where(x => x.Status == BookStatus.Available
                && (x.CategoryId == categoryId || x.Category.ParentCategoryId == categoryId));

        return await FormatCategoryBooksResultAsync(query, page, itemsPerPage);
    }

    private async Task<CategoryBooksResultDTO> FormatCategoryBooksResultAsync(IQueryable<Book> query, int page, int itemsPerPage)
    {
        var totalItems = await query.CountAsync();
        var physicalCount = await query.CountAsync(x => x.Type == BookType.Printed);
        var ebooksCount = await query.CountAsync(x => x.Type == BookType.Eletronic);

        var items = await query
            .OrderByDescending(x => x.CreationDate)
            .Skip((page - 1) * itemsPerPage)
            .Take(itemsPerPage)
            .ToListAsync();

        return new CategoryBooksResultDTO
        {
            Page = page,
            ItemsPerPage = itemsPerPage,
            TotalItems = totalItems,
            PhysicalBooksCount = physicalCount,
            EbooksCount = ebooksCount,
            Items = SetImageUrls(items)
        };
    }

    public async Task<Book?> BySlugAsync(string slug)
    {
        var pagedBook = await SearchBooksAsync(x => x.Slug != null && x.Slug.Equals(slug), 1, 1);
        return pagedBook.Items.FirstOrDefault();
    }

    public async Task<IList<Book>> GetRecommendationsAsync(Guid bookId, int limit = 6)
    {
        var normalizedLimit = Math.Min(Math.Max(limit, 1), 6);
        var source = await _repository.Get()
            .AsNoTracking()
            .Include(book => book.Category)
                .ThenInclude(category => category.ParentCategory)
            .FirstOrDefaultAsync(book => book.Id == bookId);

        if (source == null)
            throw new ShareBook.Domain.Exceptions.ShareBookException(ShareBook.Domain.Exceptions.ShareBookException.Error.NotFound);

        var candidates = await _repository.Get()
            .AsNoTracking()
            .Include(book => book.Category)
                .ThenInclude(category => category.ParentCategory)
            .Where(book => book.Status == BookStatus.Available && book.Id != bookId)
            .ToListAsync();

        var rankedIds = BookRecommendationRanker
            .Rank(source, candidates, normalizedLimit)
            .Select(book => book.Id)
            .ToList();

        if (rankedIds.Count == 0)
            return Array.Empty<Book>();

        var books = await _repository.Get()
            .AsNoTracking()
            .Include(book => book.User)
                .ThenInclude(user => user!.Address)
            .Include(book => book.Category)
                .ThenInclude(category => category.ParentCategory)
            .Where(book => rankedIds.Contains(book.Id))
            .ToListAsync();

        var booksById = books.ToDictionary(book => book.Id);
        var orderedBooks = rankedIds
            .Where(booksById.ContainsKey)
            .Select(id => booksById[id])
            .ToList();

        return SetImageUrls(orderedBooks);
    }

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
                DownloadCount = u.DownloadCount,
                FreightOption = u.FreightOption,
                ImageSlug = u.ImageSlug,
                ImageVersion = u.ImageVersion,
                ImageUrl = _uploadService.GetImageUrl(u.ImageSlug ?? string.Empty, "Books", u.ImageVersion),
                ThumbnailUrl = _uploadService.GetBookThumbnailUrl(u.ImageSlug ?? string.Empty, u.ImageVersion),
                Slug = u.Slug,
                CreationDate = u.CreationDate,
                Synopsis = u.Synopsis,
                ChooseDate = u.ChooseDate,
                User = new User()
                {
                    Id = u.User!.Id,
                    Email = u.User.Email,
                    Name = u.User.Name,
                    Linkedin = u.User.Linkedin,
                    Address = new Address()
                    {
                        City = u.User.Address!.City,
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
}
