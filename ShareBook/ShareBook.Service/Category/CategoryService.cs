using FluentValidation;
using ShareBook.Domain;
using ShareBook.Repository;
using ShareBook.Repository.UoW;
using ShareBook.Service.Generic;

using Microsoft.EntityFrameworkCore;
using ShareBook.Domain.Common;
using ShareBook.Domain.DTOs;
using ShareBook.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public class CategoryService : BaseService<Category>, ICategoryService
    {
        private readonly IBookRepository _bookRepository;

        public CategoryService(ICategoryRepository categoryRepository, 
            IBookRepository bookRepository,
            IUnitOfWork unitOfWork, 
            IValidator<Category> validator)
            : base(categoryRepository, unitOfWork, validator)
        {
            _bookRepository = bookRepository;
        }

        public async Task<PagedList<Category>> GetRootCategoriesAsync(int page, int itemsPerPage)
        {
            var query = _repository.Get()
                .Include(x => x.ParentCategory)
                .Include(x => x.Children)
                .Where(x => x.ParentCategoryId == null)
                .OrderBy(x => x.Name);

            return await FormatPagedListAsync(query, page, itemsPerPage);
        }

        public async Task<Category> FindWithHierarchyAsync(Guid categoryId)
        {
            return await _repository.Get()
                .Include(x => x.ParentCategory)
                .Include(x => x.Children)
                .FirstOrDefaultAsync(x => x.Id == categoryId);
        }

        public async Task<IEnumerable<Category>> GetCategoriesWithCountsAsync()
        {
            var categories = await _repository.Get()
                .Include(x => x.Children)
                .ToListAsync();

            var bookCounts = await _bookRepository.Get()
                .Where(b => b.Status == Domain.Enums.BookStatus.Available)
                .GroupBy(b => b.CategoryId)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CategoryId, x => x.Count);

            foreach (var category in categories)
            {
                category.TotalBooks = CalculateTotalBooks(category, categories, bookCounts);
            }

            return categories.Where(x => x.ParentCategoryId == null).OrderBy(x => x.Name);
        }

        public async Task<IList<SitemapCategoryDTO>> GetSitemapCategoriesAsync()
        {
            var categories = await _repository.Get()
                .AsNoTracking()
                .Select(category => new
                {
                    category.Id,
                    category.Name,
                    category.ParentCategoryId
                })
                .ToListAsync();

            var books = await _bookRepository.Get()
                .AsNoTracking()
                .Where(book => book.Status == BookStatus.Available)
                .Select(book => new
                {
                    book.CategoryId,
                    LastModifiedAt = book.ApprovedAt ?? book.CreationDate
                })
                .ToListAsync();

            var categoriesById = categories.ToDictionary(category => category.Id);
            var childrenByParentId = categories
                .Where(category => category.ParentCategoryId.HasValue)
                .GroupBy(category => category.ParentCategoryId.Value)
                .ToDictionary(group => group.Key, group => group.Select(category => category.Id).ToList());
            var booksByCategoryId = books
                .GroupBy(book => book.CategoryId)
                .ToDictionary(group => group.Key, group => group.ToList());
            var results = new List<SitemapCategoryDTO>();

            (int Count, DateTime? LastModifiedAt) Aggregate(Guid categoryId)
            {
                var directBooks = booksByCategoryId.TryGetValue(categoryId, out var categoryBooks)
                    ? categoryBooks
                    : null;
                var count = directBooks?.Count ?? 0;
                var lastModifiedAt = directBooks?
                    .Where(book => book.LastModifiedAt.HasValue)
                    .Select(book => book.LastModifiedAt)
                    .Max();

                if (childrenByParentId.TryGetValue(categoryId, out var childIds))
                {
                    foreach (var childId in childIds)
                    {
                        var childAggregate = Aggregate(childId);
                        count += childAggregate.Count;
                        if (childAggregate.LastModifiedAt.HasValue
                            && (!lastModifiedAt.HasValue
                                || childAggregate.LastModifiedAt.Value > lastModifiedAt.Value))
                        {
                            lastModifiedAt = childAggregate.LastModifiedAt;
                        }
                    }
                }

                return (count, lastModifiedAt);
            }

            foreach (var category in categories)
            {
                var aggregate = Aggregate(category.Id);
                if (aggregate.Count == 0)
                {
                    continue;
                }

                results.Add(new SitemapCategoryDTO
                {
                    Name = category.Name,
                    ParentCategoryName = category.ParentCategoryId.HasValue
                        && categoriesById.TryGetValue(category.ParentCategoryId.Value, out var parent)
                            ? parent.Name
                            : null,
                    LastModifiedAt = aggregate.LastModifiedAt
                });
            }

            return results.OrderBy(category => category.Name).ToList();
        }

        private int CalculateTotalBooks(Category category, List<Category> allCategories, Dictionary<Guid, int> bookCounts)
        {
            int count = bookCounts.ContainsKey(category.Id) ? bookCounts[category.Id] : 0;

            var children = allCategories.Where(x => x.ParentCategoryId == category.Id);
            foreach (var child in children)
            {
                count += CalculateTotalBooks(child, allCategories, bookCounts);
            }

            return count;
        }
    }
}
