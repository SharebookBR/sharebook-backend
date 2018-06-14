using ShareBook.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

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
