using Microsoft.EntityFrameworkCore;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Domain.Enums;
using ShareBook.Domain.Exceptions;
using ShareBook.Helper.Extensions;
using ShareBook.Repository;
using ShareBook.Service.Upload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShareBook.Service;

public class TagService(ApplicationDbContext context, IUploadService uploadService) : ITagService
{
    private const int MaxTagsPerBook = 3;
    private readonly ApplicationDbContext _context = context;
    private readonly IUploadService _uploadService = uploadService;

    public async Task<IList<Tag>> GetPublicTagsAsync()
        => await BasePublicTagsQuery()
            .OrderBy(tag => tag.Family)
            .ThenBy(tag => tag.Name)
            .ToListAsync();

    public async Task<IList<Tag>> GetAdminTagsAsync()
        => await _context.Tags
            .AsNoTracking()
            .OrderBy(tag => tag.Family)
            .ThenBy(tag => tag.Name)
            .ToListAsync();

    public async Task<Tag?> FindPublicAsync(string idOrAlias)
    {
        var normalized = NormalizeTagId(idOrAlias);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        return await BasePublicTagsQuery()
            .FirstOrDefaultAsync(tag => tag.Id == normalized || tag.Aliases.Contains(normalized));
    }

    public async Task<Tag?> FindAdminAsync(string id)
        => await _context.Tags.FindAsync(NormalizeTagId(id));

    public async Task<PagedList<Book>> GetPublicBooksAsync(string idOrAlias, int page, int itemsPerPage)
    {
        var tag = await FindPublicAsync(idOrAlias)
            ?? throw new ShareBookException(ShareBookException.Error.NotFound);

        var query = _context.Books
            .AsNoTracking()
            .Include(book => book.User)
                .ThenInclude(user => user!.Address)
            .Include(book => book.Category)
                .ThenInclude(category => category.ParentCategory)
            .Include(book => book.BookTags)
                .ThenInclude(bookTag => bookTag.Tag)
            .Where(book => book.Status == BookStatus.Available
                && book.BookTags.Any(bookTag =>
                    bookTag.TagId == tag.Id
                    && bookTag.ReviewStatus == BookTagReviewStatus.Approved
                    && bookTag.Tag.Status == TagStatus.Active
                    && bookTag.Tag.IsPublic))
            .OrderByDescending(book => book.CreationDate);

        var total = await query.CountAsync();
        var books = await query
            .Skip((page - 1) * itemsPerPage)
            .Take(itemsPerPage)
            .ToListAsync();

        return new PagedList<Book>
        {
            Page = page,
            ItemsPerPage = itemsPerPage,
            TotalItems = total,
            Items = SetImageUrls(books)
        };
    }

    public async Task<Tag> CreateAsync(Tag tag)
    {
        NormalizeTag(tag, fallbackId: tag.Id);
        await EnsureTagIsValidAsync(tag);

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        return tag;
    }

    public async Task<Tag> UpdateAsync(string id, Tag tag)
    {
        var normalizedId = NormalizeTagId(id);
        var savedTag = await _context.Tags.FindAsync(normalizedId)
            ?? throw new ShareBookException(ShareBookException.Error.NotFound);

        NormalizeTag(tag, fallbackId: normalizedId);
        if (!string.Equals(savedTag.Id, tag.Id, StringComparison.Ordinal))
        {
            throw new ShareBookException("Trocar o id/slug de uma tag ainda é uma operação explícita de migração, não um update comum.");
        }

        await EnsureTagIsValidAsync(tag, savedTag.Id);

        savedTag.Name = tag.Name;
        savedTag.Aliases = tag.Aliases;
        savedTag.Family = tag.Family;
        savedTag.Description = tag.Description;
        savedTag.UsageNotes = tag.UsageNotes;
        savedTag.Status = tag.Status;
        savedTag.IsPublic = tag.IsPublic;
        savedTag.UpdateDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return savedTag;
    }

    public async Task DeprecateAsync(string id)
    {
        var tag = await _context.Tags.FindAsync(NormalizeTagId(id))
            ?? throw new ShareBookException(ShareBookException.Error.NotFound);

        tag.Status = TagStatus.Deprecated;
        tag.IsPublic = false;
        tag.UpdateDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task<IList<BookTag>> GetBookTagsAsync(Guid bookId)
        => await _context.BookTags
            .AsNoTracking()
            .Include(bookTag => bookTag.Tag)
            .Where(bookTag => bookTag.BookId == bookId)
            .OrderBy(bookTag => bookTag.Position)
            .ToListAsync();

    public async Task<IList<BookTag>> SetBookTagsAsync(Guid bookId, IEnumerable<string> tagIds)
    {
        var bookExists = await _context.Books.AnyAsync(book => book.Id == bookId);
        if (!bookExists)
        {
            throw new ShareBookException(ShareBookException.Error.NotFound);
        }

        var normalizedTagIds = tagIds
            .Select(NormalizeTagId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (normalizedTagIds.Count > MaxTagsPerBook)
        {
            throw new ShareBookException($"Um livro pode ter no máximo {MaxTagsPerBook} tags.");
        }

        var tags = await _context.Tags
            .Where(tag => normalizedTagIds.Contains(tag.Id))
            .ToListAsync();

        var tagsById = tags.ToDictionary(tag => tag.Id);
        var missingTag = normalizedTagIds.FirstOrDefault(id => !tagsById.ContainsKey(id));
        if (missingTag != null)
        {
            throw new ShareBookException($"Tag não encontrada: {missingTag}.");
        }

        var unavailableTag = tags.FirstOrDefault(tag => tag.Status != TagStatus.Active || !tag.IsPublic);
        if (unavailableTag != null)
        {
            throw new ShareBookException($"Tag não disponível para associação pública: {unavailableTag.Id}.");
        }

        var currentBookTags = await _context.BookTags
            .Where(bookTag => bookTag.BookId == bookId)
            .ToListAsync();
        _context.BookTags.RemoveRange(currentBookTags);

        for (var index = 0; index < normalizedTagIds.Count; index++)
        {
            _context.BookTags.Add(new BookTag
            {
                BookId = bookId,
                TagId = normalizedTagIds[index],
                Position = index + 1,
                Source = BookTagSource.Manual,
                ReviewStatus = BookTagReviewStatus.Approved
            });
        }

        await _context.SaveChangesAsync();

        return await GetBookTagsAsync(bookId);
    }

    private IQueryable<Tag> BasePublicTagsQuery()
        => _context.Tags
            .AsNoTracking()
            .Where(tag => tag.Status == TagStatus.Active && tag.IsPublic);

    private async Task EnsureTagIsValidAsync(Tag tag, string? currentId = null)
    {
        if (string.IsNullOrWhiteSpace(tag.Id))
        {
            throw new ShareBookException("O id da tag é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(tag.Name))
        {
            throw new ShareBookException("O nome da tag é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(tag.Family))
        {
            throw new ShareBookException("A família da tag é obrigatória.");
        }

        if (tag.Aliases.Contains(tag.Id, StringComparer.Ordinal))
        {
            throw new ShareBookException("Alias não pode repetir o id canônico da tag.");
        }

        var duplicatedAlias = tag.Aliases
            .GroupBy(alias => alias, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1)
            ?.Key;

        if (duplicatedAlias != null)
        {
            throw new ShareBookException($"Alias duplicado na própria tag: {duplicatedAlias}.");
        }

        var allTags = await _context.Tags
            .AsNoTracking()
            .Where(existing => currentId == null || existing.Id != currentId)
            .ToListAsync();

        if (allTags.Any(existing => existing.Id == tag.Id || existing.Aliases.Contains(tag.Id)))
        {
            throw new ShareBookException($"Id de tag já usado como id ou alias: {tag.Id}.");
        }

        var conflictingAlias = tag.Aliases.FirstOrDefault(alias =>
            allTags.Any(existing => existing.Id == alias || existing.Aliases.Contains(alias)));
        if (conflictingAlias != null)
        {
            throw new ShareBookException($"Alias já usado como id ou alias em outra tag: {conflictingAlias}.");
        }
    }

    private static void NormalizeTag(Tag tag, string fallbackId)
    {
        tag.Id = NormalizeTagId(string.IsNullOrWhiteSpace(tag.Id) ? fallbackId : tag.Id);
        tag.Name = tag.Name.Trim();
        tag.Family = NormalizeTagId(tag.Family);
        tag.Aliases = tag.Aliases
            .Select(NormalizeTagId)
            .Where(alias => !string.IsNullOrWhiteSpace(alias))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static string NormalizeTagId(string value)
        => (value ?? string.Empty).ToNormalizedSearchText().GenerateSlug();

    private IList<Book> SetImageUrls(IList<Book> books)
    {
        foreach (var book in books)
        {
            book.ImageUrl = _uploadService.GetImageUrl(book.ImageSlug ?? string.Empty, "Books", book.ImageVersion);
            book.ThumbnailUrl = _uploadService.GetBookThumbnailUrl(book.ImageSlug ?? string.Empty, book.ImageVersion);
        }

        return books;
    }
}
