using FluentValidation;
using ShareBook.Domain;
using ShareBook.Repository;
using ShareBook.Repository.Infra;
using ShareBook.Service.Generic;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShareBook.Service
{
    public class CategoryService : BaseService<Category>, ICategoryService
    {
        public CategoryService(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork, IValidator<Category> validator)
            : base(categoryRepository, unitOfWork, validator)
        {
            
        }
    }
}
