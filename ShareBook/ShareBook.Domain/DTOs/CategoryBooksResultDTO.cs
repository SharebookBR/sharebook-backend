using System.Collections.Generic;

namespace ShareBook.Domain.DTOs
{
    public class CategoryBooksResultDTO
    {
        public int Page { get; set; }
        public int ItemsPerPage { get; set; }
        public int TotalItems { get; set; }
        public int PhysicalBooksCount { get; set; }
        public int EbooksCount { get; set; }
        public IList<Book> Items { get; set; }
    }
}