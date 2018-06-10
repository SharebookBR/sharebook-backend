using Moq;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Domain.Validators;
using ShareBook.Repository;
using ShareBook.Repository.Infra;
using ShareBook.Service;
using ShareBook.Service.Upload;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ShareBook.Test.Unit.Services
{
    public class BookServiceTests
    {
        readonly Mock<IBookService> bookServiceMock;
        readonly Mock<IUploadService> uploadServiceMock;
        readonly Mock<IBookRepository> bookRepositoryMock;
        readonly Mock<IBooksEmailService> bookEmailService;
        readonly Mock<IUnitOfWork> unitOfWorkMock;

        public BookServiceTests()
        {
            // Definindo quais serão as classes mockadas
            bookServiceMock = new Mock<IBookService>();
            uploadServiceMock = new Mock<IUploadService>();
            unitOfWorkMock = new Mock<IUnitOfWork>();
            bookRepositoryMock = new Mock<IBookRepository>();
            bookEmailService = new Mock<IBooksEmailService>();

            bookRepositoryMock.Setup(repo => repo.Insert(It.IsAny<Book>())).Returns(() =>
            {
                return new Book()
                {
                    Title = "Lord of the Rings",
                    Author = "J. R. R. Tolkien",
                    Image = "lotr.png",
                    ImageBytes = Encoding.UTF8.GetBytes("STRINGBASE64"),
                    UserId = new Guid("5489A967-9320-4350-E6FC-08D5CC8498F3"),
                };
            });

            uploadServiceMock.Setup(service => service.UploadImage(null, null));

            bookServiceMock.Setup(service => service.Insert(It.IsAny<Book>())).Verifiable();
        }

        [Fact]
        public void AddBook()
        {
            var service = new BookService(bookRepositoryMock.Object, unitOfWorkMock.Object, new BookValidator(), uploadServiceMock.Object, bookEmailService.Object);
            Result<Book> result = service.Insert(new Book()
            {
                Title = "Lord of the Rings",
                Author = "J. R. R. Tolkien",
                Image = "lotr.png",
                ImageBytes = Encoding.UTF8.GetBytes("STRINGBASE64"),
                UserId = new Guid("5489A967-9320-4350-E6FC-08D5CC8498F3"),
            });
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

    }
}
