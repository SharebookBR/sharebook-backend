using System;
namespace ShareBook.Api.ViewModels
{
    public class BookVMAdm
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Winner { get; set; }
        public string Donor { get; set; }
        public Guid? UserIdFacilitator { get; set; }
        public string Facilitator { get; set; }
        public string FacilitatorNotes { get; set; }
        public string PhoneDonor { get; set; }
        public int DaysInShowcase { get; set; }
        public int TotalInterested { get; set; }
        public string Status { get; set; }
        public string TrackingNumber { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? ChooseDate { get; set; }
        public string FreightOption { get; set; }
        public Guid CategoryId { get; set; }
        public string Category { get; set; }
        public string ImageSlug { get; set; }
        public string ImageUrl { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Synopsis { get; set; }
        public string Slug { get; set; }
        public Guid? UserId { get; set; }
    }

    public class BookVM
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Status { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? ChooseDate { get; set; }
        public string FreightOption { get; set; }
        public Guid CategoryId { get; set; }
        public string Category { get; set; }
        public string ImageSlug { get; set; }
        public string ImageUrl { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Synopsis { get; set; }
        public string Slug { get; set; }
    }
}
