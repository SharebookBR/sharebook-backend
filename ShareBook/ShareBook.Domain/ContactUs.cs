using ShareBook.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShareBook.Domain
{
    public class ContactUs : BaseEntity
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Message { get; set; }
    }
}
