using Moq;
using ShareBook.Domain.Validators;
using ShareBook.Repository;
using ShareBook.Repository.UoW;
using ShareBook.Service;
using ShareBook.Service.Muambator;
using ShareBook.Test.Unit.Mocks;
using System;
using System.Threading;
using Xunit;

namespace ShareBook.Test.Unit.Services
{
    public class BookUserServiceTests
    {
        private Guid bookId;

        readonly Mock<IBookService> bookServiceMock;
        readonly Mock<IBookUserRepository> bookUserRepositoryMock;
        readonly Mock<IBooksEmailService> bookEmailService;
        readonly Mock<IUnitOfWork> unitOfWorkMock;
        readonly Mock<IBookUsersEmailService> bookUsersEmailService;
        readonly BookUserValidator bookUserValidator;
        readonly Mock<IMuambatorService> muambatorServiceMock;
        readonly Mock<IBookRepository> bookRepositoryMock;


        public BookUserServiceTests()
        {
            bookId = new Guid("5489A967-9320-4350-E6FC-08D5CC8498F3");
            bookServiceMock = new Mock<IBookService>();
            bookUserRepositoryMock = new Mock<IBookUserRepository>();
            bookEmailService = new Mock<IBooksEmailService>();
            unitOfWorkMock = new Mock<IUnitOfWork>();
            bookUsersEmailService = new Mock<IBookUsersEmailService>();
            muambatorServiceMock = new Mock<IMuambatorService>();
            bookRepositoryMock = new Mock<IBookRepository>();

            bookServiceMock.SetReturnsDefault(true);
        }

        [Fact]
        public void RequestBook()
        {
            Thread.CurrentPrincipal = new UserMock().GetClaimsUser();
            var service = new BookUserService(bookUserRepositoryMock.Object,
                bookServiceMock.Object, bookUsersEmailService.Object, muambatorServiceMock.Object, bookRepositoryMock.Object, 
                unitOfWorkMock.Object, bookUserValidator);

            string reason = "I need this book because I'm learning a new programming language.";

            service.Insert(bookId, reason);

        }
    }
}
