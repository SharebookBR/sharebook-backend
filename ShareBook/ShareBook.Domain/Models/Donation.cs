using System;
using ShareBook.Domain.Enums;

namespace ShareBook.Domain.Models
{
    public class Donation
    {
        public string Book;

        public Int32 DaysInShowcase;

        public Int32 TotalInterested;

        public BookStatus Status;
    }
}