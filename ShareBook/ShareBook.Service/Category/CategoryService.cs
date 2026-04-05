using FluentValidation;
using ShareBook.Domain;
using ShareBook.Repository;
using ShareBook.Repository.UoW;
using ShareBook.Service.Generic;

using Microsoft.EntityFrameworkCore;
using ShareBook.Domain.Common;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public class CategoryService : BaseService<Category>, ICategoryService
    {
        public CategoryService(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork, IValidator<Category> validator)
            : base(categoryRepository, unitOfWork, validator)
        {
            
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
    }
}
