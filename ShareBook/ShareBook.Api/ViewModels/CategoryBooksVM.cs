using ShareBook.Domain.Common;
using System.Collections.Generic;

namespace ShareBook.Api.ViewModels
{
    public class CategoryBooksVM
    {
        public int Page { get; set; }
        public int ItemsPerPage { get; set; }
        public int TotalItems { get; set; }
        public int PhysicalBooksCount { get; set; }
        public int EbooksCount { get; set; }
        public IList<BookVM> Items { get; set; }
    }
}