using ShareBook.Domain.Common;

using System;
using System.Collections.Generic;

namespace ShareBook.Domain
{
    public class Category : BaseEntity
    {
        public string Name { get; set; }

        public Guid? ParentCategoryId { get; set; }

        public Category ParentCategory { get; set; }

        public ICollection<Category> Children { get; set; } = new List<Category>();
    }
}
