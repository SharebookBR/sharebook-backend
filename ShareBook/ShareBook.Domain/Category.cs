using System;
using ShareBook.Domain.Common;

namespace ShareBook.Domain
{
    public class Category : BaseEntity
    {
        public Category()
        {
                
        }

        public Category(Guid id)
        {
            this.Id = id;
        }

        public string Name { get; set; }
    }
}
