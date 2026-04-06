using System.Collections.Generic;

namespace ShareBook.Domain.DTOs
{
    public class AdminBooksResultDTO
    {
        public int Page { get; set; }
        public int ItemsPerPage { get; set; }
        public int TotalItems { get; set; }
        public AdminBooksSummaryDTO Summary { get; set; }
        public IList<Book> Items { get; set; }
    }

    public class AdminBooksSummaryDTO
    {
        public int All { get; set; }
        public int NeedsAction { get; set; }
        public int Shipping { get; set; }
        public int Physical { get; set; }
        public int Ebooks { get; set; }
        public int Finished { get; set; }
        public int Available { get; set; }
    }
}
