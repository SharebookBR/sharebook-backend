using System;
namespace ShareBook.Api.ViewModels
{
    public class BooksVM
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Winner { get; set; }
        public string Donor { get; set; }
        public string Facilitator { get; set; }
        public string FacilitatorNotes { get; set; }
        public string PhoneDonor { get; set; }
        public int DaysInShowcase { get; set; }
        public int TotalInterested { get; set; }
        public string Status { get; set; }
        public string TrackingNumber { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? ChooseDate { get; set; }
    }
}
