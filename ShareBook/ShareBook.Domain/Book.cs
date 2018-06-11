using ShareBook.Domain.Common;
using System;

namespace ShareBook.Domain
{
    public class Book : BaseEntity
    {
        public string Title { get; set; }

        public string Author { get; set; }

        public byte[] ImageBytes { get; set; }

        public string Image { get; set; }

        public string FreightOption { get; set; }

        public Guid UserId { get; set; }

        public User User { get; set; }

        public bool Approved { get; set; } = false;
    }
}
