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

        [Theory]
        [InlineData(BookType.Eletronic ,"downloadLink", null)]
        [InlineData(BookType.Eletronic, "downloadLink", "")]
        [InlineData(BookType.Eletronic, null, "ebookPdfFile")]
        [InlineData(BookType.Eletronic, "", "ebookPdfFile")]
        [InlineData(BookType.Eletronic, "downloadLink", "ebookPdfFile")]
        public void BookTypeShouldBeEletronicIfNemBookIsCreatedWithEBookDownloadLinkOrEbookPdfFile(BookType bookType, string downloadLink, string eBookPdfFile)
        {
            var book = new Book() { Type = bookType, EBookDownloadLink = downloadLink, EBookPdfFile = eBookPdfFile};
            Assert.Equal(bookType, book.Type);
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