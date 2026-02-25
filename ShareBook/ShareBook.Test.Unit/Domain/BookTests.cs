using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Helper;
using System;
using System.Collections.Generic;
using Xunit;

namespace ShareBook.Test.Unit.Domain
{
    public class BookTests
    {
        [Fact]
        public void BookStatusWaitingApproval()
        {
            var book = new Book();
            Assert.Equal(BookStatus.WaitingApproval, book.Status);
        }

        [Fact]
        public void TotalInteresedShouldBeZeroIfBookUsersIsNull()
        {
            var book = new Book();
            Assert.Equal(0, book.TotalInterested());
        }

        [Fact]
        public void TotalInteresedShouldBeEqualsTheBookUsersLength()
        {
            var bookUsers = new List<BookUser>();
            bookUsers.Add(new BookUser());
            bookUsers.Add(new BookUser());

            var book = new Book
            {
                BookUsers = bookUsers
            };

            Assert.Equal(2, book.TotalInterested());
        }

        [Fact]
        public void DaysInShowCaseShouldBeTheDifferenceBetweenTodayAndTheCreationDate()
        {
            int expectedDays = 5;
            var book = new Book
            {
                CreationDate = DateTime.UtcNow.AddDays(expectedDays * -1)
            };

            Assert.Equal(expectedDays, book.DaysInShowcase());
        }

        [Fact]
        public void MayNotChooseWinnerAvailable()
        {
            var book = new Book { Status = BookStatus.Available };
            Assert.False(book.MayChooseWinner());
        }

        [Fact]
        public void MayChooseWinnerWhenAwaitingDonorDecision()
        {
            var book = new Book { Status = BookStatus.AwaitingDonorDecision };
            Assert.True(book.MayChooseWinner());
        }

        [Fact]
        public void MayNotChooseWinnerWaitingSend()
        {
            var book = new Book { Status = BookStatus.WaitingSend };
            Assert.False(book.MayChooseWinner());
        }

        [Fact]
        public void BookTypeShouldBeEletronicWhenSetExplicitly()
        {
            var book = new Book() { Type = BookType.Eletronic };
            Assert.Equal(BookType.Eletronic, book.Type);
        }

        [Fact]
        public void HasPdfToUploadShouldReturnTrueForEbookWithPdfBytes()
        {
            var book = new Book()
            {
                Type = BookType.Eletronic,
                PdfBytes = new byte[] { 1, 2, 3 }
            };
            Assert.True(book.HasPdfToUpload());
        }

        [Fact]
        public void HasPdfToUploadShouldReturnFalseForPrintedBook()
        {
            var book = new Book()
            {
                Type = BookType.Printed,
                PdfBytes = new byte[] { 1, 2, 3 }
            };
            Assert.False(book.HasPdfToUpload());
        }

        [Fact]
        public void IsEbookShouldReturnTrueForEletronicType()
        {
            var book = new Book() { Type = BookType.Eletronic };
            Assert.True(book.IsEbook());
        }

        [Fact]
        public void IsEbookShouldReturnFalseForPrintedType()
        {
            var book = new Book() { Type = BookType.Printed };
            Assert.False(book.IsEbook());
        }

        [Fact]
        public void GetPdfFileNameShouldReturnSlugWithPdfExtension()
        {
            var book = new Book() { Slug = "clean-code" };
            Assert.Equal("clean-code.pdf", book.GetPdfFileName());
        }

        [Fact]
        public void BookTypeShouldBePrintedIfNemBookIsCreatedByDefault()
        {
            BookType expected = BookType.Printed;
            var book = new Book();
            Assert.Equal(expected, book.Type);
        }

    }
}