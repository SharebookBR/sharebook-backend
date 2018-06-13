using Moq;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Domain.Enums;
using ShareBook.Domain.Validators;
using ShareBook.Repository;
using ShareBook.Repository.Infra;
using ShareBook.Service;
using ShareBook.Service.Upload;
using ShareBook.Test.Unit.Mocks;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using Xunit;

namespace ShareBook.Test.Unit.Services
{
    public class BookServiceTests
    {
        readonly Mock<IBookService> bookServiceMock;
        readonly Mock<IUploadService> uploadServiceMock;
        readonly Mock<IBookRepository> bookRepositoryMock;
        readonly Mock<IUnitOfWork> unitOfWorkMock;

        public BookServiceTests()
        {
            // Definindo quais serão as classes mockadas
            bookServiceMock = new Mock<IBookService>();
            uploadServiceMock = new Mock<IUploadService>();
            unitOfWorkMock = new Mock<IUnitOfWork>();
            bookRepositoryMock = new Mock<IBookRepository>();

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
            Thread.CurrentPrincipal = new UserMock().GetClaimsUser();
            var service = new BookService(bookRepositoryMock.Object, unitOfWorkMock.Object, new BookValidator(), uploadServiceMock.Object);
            Result<Book> result = service.Insert(new Book()
            {
                Title = "Lord of the Rings",
                Author = "J. R. R. Tolkien",
                Image = "lotr.png",
                ImageBytes = Encoding.UTF8.GetBytes("STRINGBASE64"),
                FreightOption = FreightOption.City
            });
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

    }
}
