using ShareBook.Domain.Enums;
using System;
namespace ShareBook.Api.ViewModels
{
    public class BooksVM
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public bool Donated { get; set; }
        public string Donor { get; set; }
        public bool Approved { get; set; }
        public string PhoneDonor { get; set; }
    }
}
