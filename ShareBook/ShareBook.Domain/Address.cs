using ShareBook.Domain.Common;
using System;

namespace ShareBook.Domain
{
    public class Address : BaseEntity
    {
        public string Street { get; set; }

        public string Number { get; set; }

        public string Complement { get; set; }

        public string Neighborhood { get; set; }

        public string PostalCode { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Country { get; set; }

        //public User User { get; set; }

        public Guid UserId { get; set; }
    }
}
