using ShareBook.Service.Generic;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public interface ICategoryService : IBaseService<Category>
    {
        Task<PagedList<Category>> GetRootCategoriesAsync(int page, int itemsPerPage);

        Task<Category> FindWithHierarchyAsync(Guid categoryId);

        Task<IEnumerable<Category>> GetCategoriesWithCountsAsync();

        Task<IList<SitemapCategoryDTO>> GetSitemapCategoriesAsync();
    }
}
