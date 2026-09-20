using Microsoft.EntityFrameworkCore;
using ShareBook.Domain;
using ShareBook.Domain.DTOs;
using ShareBook.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShareBook.Service;

public partial class BookService
{
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
            Items = SetImageUrls(books)
        };
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
            Items = SetImageUrls(items)
        };
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
}
