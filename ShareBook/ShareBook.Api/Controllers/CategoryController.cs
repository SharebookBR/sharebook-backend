using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Api.ViewModels;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareBook.Api.Controllers
{
    [Route("api/[controller]")]
    public class CategoryController : BaseCrudController<Category, CategoryVM, Category>
    {
        public CategoryController(ICategoryService categoryService, IMapper mapper)
            : base(categoryService, mapper)
        {
            SetDefault(x => x.Name);
        }

        [HttpGet]
        public new async Task<PagedList<CategoryVM>> GetAllAsync() => await PagedAsync(1, 200);

        [HttpGet("{page}/{items}")]
        public new async Task<PagedList<CategoryVM>> PagedAsync(int page, int items)
        {
            var pagedCategories = await ((ICategoryService)_service).GetRootCategoriesAsync(page, items);
            var categories = _mapper.Map<List<CategoryVM>>(pagedCategories.Items);

            return new PagedList<CategoryVM>
            {
                Page = pagedCategories.Page,
                ItemsPerPage = pagedCategories.ItemsPerPage,
                TotalItems = pagedCategories.TotalItems,
                Items = categories
            };
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var category = await ((ICategoryService)_service).FindWithHierarchyAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<CategoryVM>(category));
        }
    }
}
