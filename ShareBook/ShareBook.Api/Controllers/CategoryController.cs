using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Service;
using System.Threading.Tasks;

namespace ShareBook.Api.Controllers
{
    [Route("api/[controller]")]
    public class CategoryController : BaseCrudController<Category>
    {
        public CategoryController(ICategoryService categoryService,
            IMapper mapper) : base(categoryService, mapper)
        {
            SetDefault(x => x.Name);
        }

        public override async Task<PagedList<Category>> GetAllAsync() => await PagedAsync(1, 50);
    }
}