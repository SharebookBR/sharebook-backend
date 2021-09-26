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

        public Guid UserId { get; set; }

        public void Clear() {
            ClearStreet();
            ClearNumber();
            ClearComplement();
            ClearNeighborhood();
            ClearPostalCode();
            ClearCity();
            ClearState();
            ClearCountry();
        }

        private void ClearStreet() {
            Street = string.Empty;
        }

        private void ClearNumber() {
            Number = string.Empty;
        }

        private void ClearComplement() {
            Complement = string.Empty;
        }

        private void ClearNeighborhood() {
            Neighborhood = string.Empty;
        }

        private void ClearPostalCode() {
            PostalCode = string.Empty;
        }

        private void ClearCity() {
            City = string.Empty;
        }

        private void ClearState() {
            State = string.Empty;
        }

        private void ClearCountry() {
            Country = string.Empty;
        }
    }
}