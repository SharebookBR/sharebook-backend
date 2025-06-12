using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using ShareBook.Domain;
using ShareBook.Domain.Validators;
using ShareBook.Repository;
using ShareBook.Repository.UoW;
using ShareBook.Service;
using ShareBook.Service.Muambator;
using ShareBook.Test.Unit.Mocks;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ShareBook.Test.Unit.Services
{
    public class BookUserServiceTests
    {
        private Guid bookId;

        readonly Mock<IBookService> bookServiceMock = new();
        readonly Mock<IBookUserRepository> bookUserRepositoryMock = new();
        readonly Mock<IBooksEmailService> bookEmailService = new();
        readonly Mock<IUnitOfWork> unitOfWorkMock = new();
        readonly Mock<IBookUsersEmailService> bookUsersEmailService = new();
        readonly Mock<BookUserValidator> bookUserValidator = new();
        readonly Mock<IMuambatorService> muambatorServiceMock = new();
        readonly Mock<IBookRepository> bookRepositoryMock = new();
        readonly Mock<IConfiguration> configurationMock = new();
        readonly Mock<IHttpContextAccessor> httpContextAccessorMock = new();


        public BookUserServiceTests()
        {
            bookId = new Guid("5489A967-9320-4350-E6FC-08D5CC8498F3");

            configurationMock.Setup(c => c["SharebookSettings:MaxRequestsPerBook"]).Returns("50");

            bookServiceMock.SetReturnsDefault(true);

            bookServiceMock.Setup(s => s.AnyAsync(It.IsAny<Expression<Func<Book, bool>>>())).ReturnsAsync(true);
            bookServiceMock.Setup(s => s.GetBookWithAllUsersAsync(It.IsAny<Guid>())).ReturnsAsync(() =>
            {
                return BookMock.GetLordTheRings();
            });
        }

        [Fact]
        public async Task RequestBook()
        {
            Thread.CurrentPrincipal = new UserMock().GetClaimsUser();
            var service = new BookUserService(bookUserRepositoryMock.Object,
                bookServiceMock.Object, bookUsersEmailService.Object, muambatorServiceMock.Object, bookRepositoryMock.Object,
                unitOfWorkMock.Object, bookUserValidator.Object, configurationMock.Object, httpContextAccessorMock.Object);


            string reason = "I need this book because I'm learning a new programming language.";

            await service.InsertAsync(bookId, reason);

            // TODO: Verify test and add at least one assertion
        }
    }
}
