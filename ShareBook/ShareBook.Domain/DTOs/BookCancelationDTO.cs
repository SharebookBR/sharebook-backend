using System;

namespace ShareBook.Domain.DTOs
{
    public class BookCancelationDTO
    {
        public Book Book { get; set; }
        public string CanceledBy { get; set; }
        public string Reason { get; set; }
    }
}
