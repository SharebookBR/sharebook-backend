using System.Collections.Generic;

namespace ShareBook.Domain.DTOs
{
    public class UserDonationsResultDTO
    {
        public int Page { get; set; }
        public int ItemsPerPage { get; set; }
        public int TotalItems { get; set; }
        public UserDonationsSummaryDTO Summary { get; set; }
        public IList<Book> Items { get; set; }
    }

    public class UserDonationsSummaryDTO
    {
        public int WaitingDecision { get; set; }
        public int WaitingSend { get; set; }
        public int Finished { get; set; }
        public int EbookDownloadsTotal { get; set; }
    }
}
