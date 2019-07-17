using Microsoft.AspNetCore.Mvc;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Service;

namespace ShareBook.Api.Controllers
{
    [Route("api/[controller]")]
    public class CategoryController : BaseCrudController<Category>
    {
        public CategoryController(ICategoryService categoryService) : base(categoryService)
        {
            SetDefault(x => x.Name);
        }

        public override PagedList<Category> GetAll() => Paged(1, 50);
    }
}
