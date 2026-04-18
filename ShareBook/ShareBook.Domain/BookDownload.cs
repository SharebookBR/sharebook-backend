using ShareBook.Domain.Common;
using System;

namespace ShareBook.Domain
{
    public class BookDownload : BaseEntity
    {
        public Guid BookId { get; set; }
        public Book Book { get; set; }

        public Guid? UserId { get; set; }
        public User User { get; set; }

        public DateTime DownloadedAt { get; set; } = DateTime.UtcNow;

        public string UserAgent { get; set; }

        public string IpAddress { get; set; }

        public bool IsLoggedIn => UserId.HasValue;
    }
}