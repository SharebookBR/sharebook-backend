using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Service;

namespace ShareBook.Api.Controllers {
    [Route ("api/[controller]")]
    public class CategoryController : BaseCrudController<Category> {
        private readonly IMapper _mapper;
        public CategoryController (ICategoryService categoryService,
            IMapper mapper) : base (categoryService, mapper) {
            _mapper = mapper;
            SetDefault (x => x.Name);
        }

        public override PagedList<Category> GetAll () => Paged (1, 50);
    }
}