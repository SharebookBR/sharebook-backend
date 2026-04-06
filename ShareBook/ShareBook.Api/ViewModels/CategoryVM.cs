using ShareBook.Domain.Common;
using System;
using System.Collections.Generic;

namespace ShareBook.Api.ViewModels
{
    public class CategoryVM : IIdProperty
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public string ParentCategoryName { get; set; }
        public IList<CategoryVM> Children { get; set; } = new List<CategoryVM>();
    }
}
