using ShareBook.Domain.Common;
using ShareBook.Domain.Enums;
using System;

namespace ShareBook.Domain
{
    public class BookUser : BaseEntity
    {
        public Guid BookId { get; set; }
        public Book Book { get; set; }
        public User User { get; set; }
        public Guid UserId { get; set; }
        public DonationStatus Status { get; set; } = DonationStatus.WaitingAction;

    }
}
