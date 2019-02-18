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
        public string NickName { get; set; }
        public DonationStatus Status { get; private set; } = DonationStatus.WaitingAction;
        public string Note { get; set; } // motivo do doador ter escolhido.
        public string Reason { get; set; } // justificativa do interessado.

        public void UpdateBookUser(DonationStatus status, string note)
        {
            this.Status = status;
            this.Note = note;
        }

    }
}
