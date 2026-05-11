using FluentValidation;
using ShareBook.Domain;
using ShareBook.Repository;
using ShareBook.Repository.UoW;
using ShareBook.Service.Generic;

using Microsoft.EntityFrameworkCore;
using ShareBook.Domain.Common;
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
