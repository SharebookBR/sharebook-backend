using ShareBook.Domain.Common;
using ShareBook.Domain.Enums;
using System;
using System.Collections.Generic;
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

        public Guid CategoryId { get; set; }

        public User User { get; set; }

        public Category Category { get; set; }

        public bool Approved { get; set; } = false;

        public virtual ICollection<BookUser> BookUsers { get; set; }

        public string ImageUrl { get; set; }

        public string ImageName { get; set; }

        public bool Donated()
            => !BookUsers.Any(x => x.Status == DonationStatus.WaitingAction);
    }
}
