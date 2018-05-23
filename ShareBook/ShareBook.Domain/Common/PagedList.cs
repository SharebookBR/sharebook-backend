using System.Collections.Generic;

namespace ShareBook.Domain.Common
{
    public class PagedList<T>
    {
        public int Page { get; set; }
        public int ItemsPerPage { get; set; }
        public int TotalItems { get; set; }
        public IList<T> Items { get; set; }
    }
}
