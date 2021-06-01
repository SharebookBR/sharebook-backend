using ShareBook.Domain.Common;
using ShareBook.Domain.Enums;
using ShareBook.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace ShareBook.Domain
{
    public class Book : BaseEntity
    {
        public string Title { get; set; }

        public string Author { get; set; }

        public string Slug { get; set; }

        public byte[] ImageBytes { get; set; }

        public string ImageSlug { get; set; }

        public FreightOption FreightOption { get; set; }

        public Guid? UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } // Doador.

        public Guid? UserIdFacilitator { get; set; }

        [ForeignKey("UserIdFacilitator")]
        public User UserFacilitator { get; set; } // Facilitador.

        public string FacilitatorNotes { get; set; }

        public Guid CategoryId { get; set; }

        public Category Category { get; set; }

        public virtual ICollection<BookUser> BookUsers { get; set; }

        public string ImageUrl { get; set; }

        public string ImageName { get; set; }

        public DateTime? ChooseDate { get; set; }

        public string Synopsis { get; set; }

        public string TrackingNumber { get; set; }

        public BookStatus Status { get; set; }

        public BookType Type { get; set; } = BookType.Printed;

        public string EBookDownloadLink { get; set; }
        public string EBookPdfFile { get; set; }
        public byte[] EBookPdfBytes {get; set; }

        public Book()
        {
           Status = BookStatus.WaitingApproval;
        }

        public string WinnerName()
            => BookUsers?.FirstOrDefault(x => x.Status == DonationStatus.Donated)?.User?.Name ?? "";

        public User WinnerUser()
            => BookUsers?.FirstOrDefault(x => x.Status == DonationStatus.Donated)?.User;

        public int TotalInterested()
        {
            return BookUsers?.Count ?? 0;
        }

        public int DaysInShowcase()
        {
            TimeSpan diff = (TimeSpan)(DateTime.Now - this.CreationDate);
            return diff.Days;
        }

        public int DaysLate()
        {
            if (Status != BookStatus.AwaitingDonorDecision) return 0;

            if (ChooseDate >= DateTime.Today) return 0;

            TimeSpan diff = (TimeSpan)(DateTime.Today - ChooseDate);
            return diff.Days;
        }

        public bool MayChooseWinner()
        {
            return Status == BookStatus.AwaitingDonorDecision;
        }

        public bool IsEbookPdfValid()
        {
            return Type == BookType.Eletronic
                && EBookPdfBytes != null
                && EBookPdfBytes.Length > 0;
        }


        
    }
}
