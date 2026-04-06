using System;
using System.Collections.Generic;
namespace ShareBook.Api.ViewModels
{
    public class BookCategoryVM
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public string ParentCategoryName { get; set; }
    }

    public class BookVMAdm
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int DownloadCount { get; set; }
        public string Winner { get; set; }
        public string Donor { get; set; }
        public Guid? UserIdFacilitator { get; set; }
        public string Facilitator { get; set; }
        public string FacilitatorNotes { get; set; }
        public string PhoneDonor { get; set; }
        public int DaysInShowcase { get; set; }
        public int DaysLate { get; set; }
        public int TotalInterested { get; set; }
        public string Status { get; set; }
        public string TrackingNumber { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? ChooseDate { get; set; }
        public string FreightOption { get; set; }
        public Guid CategoryId { get; set; }
        public string Category { get; set; }
        public BookCategoryVM CategoryInfo { get; set; }
        public string ImageSlug { get; set; }
        public string ImageUrl { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Synopsis { get; set; }
        public string Slug { get; set; }
        public Guid? UserId { get; set; }
        public string Type { get; set; }
        public string EBookPdfPath { get; set; }
    }

    public class AdminBooksSummaryVM
    {
        public int All { get; set; }
        public int NeedsAction { get; set; }
        public int Shipping { get; set; }
        public int Physical { get; set; }
        public int Ebooks { get; set; }
        public int Finished { get; set; }
        public int Available { get; set; }
    }

    public class AdminBooksPagedVM
    {
        public int Page { get; set; }
        public int ItemsPerPage { get; set; }
        public int TotalItems { get; set; }
        public AdminBooksSummaryVM Summary { get; set; }
        public IList<BookVMAdm> Items { get; set; }
    }

    public class UserDonationsSummaryVM
    {
        public int WaitingDecision { get; set; }
        public int WaitingSend { get; set; }
        public int Finished { get; set; }
        public int EbookDownloadsTotal { get; set; }
    }

    public class UserDonationsPagedVM
    {
        public int Page { get; set; }
        public int ItemsPerPage { get; set; }
        public int TotalItems { get; set; }
        public UserDonationsSummaryVM Summary { get; set; }
        public IList<BookVMAdm> Items { get; set; }
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
        public BookCategoryVM CategoryInfo { get; set; }
        public string ImageSlug { get; set; }
        public string ImageUrl { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Synopsis { get; set; }
        public string Slug { get; set; }
        public string Type { get; set; }
        public string EBookPdfPath { get; set; }
        public BookDonorVM Donor { get; set; }
    }

    public class BookDonorVM
    {
        public string DisplayName { get; set; }
        public string Linkedin { get; set; }
    }
}
