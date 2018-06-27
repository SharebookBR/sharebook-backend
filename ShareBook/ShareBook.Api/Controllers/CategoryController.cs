using ShareBook.Domain;
using ShareBook.Service;

namespace ShareBook.Api.Controllers
{
    public class CategoryController : BaseController<Category>
    {
        public CategoryController(ICategoryService categoryService) : base(categoryService)
        {
            SetDefault(x => x.Name);
        }
    }
}
