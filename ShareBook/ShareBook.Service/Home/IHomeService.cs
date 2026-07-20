using ShareBook.Domain.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareBook.Service.Home
{
    public interface IHomeService
    {
        Task<List<HomeShowcaseBookDTO>> GetFeaturedPrintedBooksAsync();
        Task<List<HomeShowcaseCategoryDTO>> GetCategoriesShowcaseAsync();
    }
}
