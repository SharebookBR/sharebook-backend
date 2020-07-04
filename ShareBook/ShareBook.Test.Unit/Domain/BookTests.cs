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
                CreationDate = DateTime.Now.AddDays(expectedDays * -1)
            };

            Assert.Equal(expectedDays, book.DaysInShowcase());
        }

        [Fact]
        public void MayChooseWinnerWhenChooseDateIsYesterday()
        {
            var book = new Book { ChooseDate = DateTimeHelper.GetTodaySaoPaulo().AddDays(-1) };
            Assert.True(book.MayChooseWinner());
        }

        [Fact]
        public void MayChooseWinnerWhenChooseDateIsToday()
        {
            var book = new Book { ChooseDate = DateTimeHelper.GetTodaySaoPaulo() };
            Assert.True(book.MayChooseWinner());
        }

        [Fact]
        public void MayNotChooseWinnerWhenChooseDateIsTomorrow()
        {
            var book = new Book { ChooseDate = DateTimeHelper.GetTodaySaoPaulo().AddDays(1) };
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

        [Theory]
        [InlineData("", "")]
        [InlineData(null, null)]
        public void BookTypeShouldBePhysicalIfNemBookIsCreatedByDefault(string downloadLink, string eBookPdfFile)
        {
            BookType expected = BookType.Printed;
            var book = new Book() { EBookDownloadLink = downloadLink, EBookPdfFile = eBookPdfFile };
            var book2 = new Book();
            Assert.Equal(expected, book.Type);
            Assert.Equal(expected, book2.Type);
        }

    }
}