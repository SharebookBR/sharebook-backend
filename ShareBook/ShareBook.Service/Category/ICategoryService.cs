using ShareBook.Service.Generic;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using System;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public interface ICategoryService : IBaseService<Category>
    {
        Task<PagedList<Category>> GetRootCategoriesAsync(int page, int itemsPerPage);

        Task<Category> FindWithHierarchyAsync(Guid categoryId);
    }
}
