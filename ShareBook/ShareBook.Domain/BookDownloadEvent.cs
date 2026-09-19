using ShareBook.Domain.Common;
using ShareBook.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShareBook.Domain
{
    public class BookDownloadEvent : BaseEntity
    {
        public Guid BookId { get; set; }

        public Book Book { get; set; }

        public Guid? UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        public DateTime DownloadedAtUtc { get; set; } = DateTime.UtcNow;

        public BookDownloadEventSource Source { get; set; } = BookDownloadEventSource.Live;
    }
}
