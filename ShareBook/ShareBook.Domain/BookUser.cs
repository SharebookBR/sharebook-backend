using System;

namespace ShareBook.Domain
{
    public class BookUser
    {
        public Guid BookId { get; set; }
        public Book Book { get; set; }
        public User User { get; set; }
        public Guid UserId { get; set; }
        
    }
}
