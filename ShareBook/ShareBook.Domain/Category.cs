using ShareBook.Domain.Common;
using System.Collections.Generic;

namespace ShareBook.Domain
{
    public class Category : BaseEntity
    {
        public string Name { get; set; }

        public ICollection<Book> Books { get; set; }
    }
}
