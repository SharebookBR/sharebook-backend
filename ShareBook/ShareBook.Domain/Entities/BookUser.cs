using ShareBook.Domain.Common;
using ShareBook.Domain.Enums;
using System;

namespace ShareBook.Domain.Entities
{
    public class BookUser : BaseEntity
    {
        public Guid BookId { get; set; }
        public Book Book { get; set; }
        public User User { get; set; }
        public Guid UserId { get; set; }
        public DonationStatus Status { get; private set; } = DonationStatus.WaitingAction;
        public string Note { get; set; }
        public string Reason { get; set; }

        public void UpdateBookUser(DonationStatus status, string note)
        {
            this.Status = status;
            this.Note = note;
        }

    }
}
