using System;
using ShareBook.Domain.Common;

namespace ShareBook.Domain
{
    public class EbookComplaint : BaseEntity
    {
        public Guid BookId { get; set; }
        public Book Book { get; set; }
        public User User { get; set; }
        public Guid UserId { get; set; }

        public EbookComplaint(Guid bookId)
        {
            BookId = bookId;
        }
    }
}