using System;

namespace ShareBook.Api.ViewModels
{
    public class CancelBookDonationVM
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Status { get; set; }
    }
}
