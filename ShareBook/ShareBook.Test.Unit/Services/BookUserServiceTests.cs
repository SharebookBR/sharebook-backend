using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using ShareBook.Domain;
using ShareBook.Domain.Common;
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

namespace ShareBook.Test.Unit.Services;

public class BookUserServiceTests
{
    private Guid bookId;

    readonly Mock<IBookService> bookServiceMock = new();
    readonly Mock<IBooksEmailService> bookEmailService = new();
    readonly Mock<IUnitOfWork> unitOfWorkMock = new();
    readonly Mock<IBookUsersEmailService> bookUsersEmailService = new();
    readonly Mock<BookUserValidator> bookUserValidator = new();
    readonly Mock<IMuambatorService> muambatorServiceMock = new();
    readonly Mock<IBookRepository> bookRepositoryMock = new();
    readonly Mock<IConfiguration> configurationMock = new();
    readonly Mock<ICurrentUserAccessor> currentUserAccessorMock = new();


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

        var currentUserId = new Guid(new UserMock().GetClaimsUser().Identity!.Name!);
        currentUserAccessorMock.Setup(x => x.UserId).Returns(currentUserId);
        currentUserAccessorMock.Setup(x => x.RequireUserId()).Returns(currentUserId);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task RequestBook()
    {
        await using var context = CreateContext();
        var service = new BookUserService(context,
            bookServiceMock.Object, bookUsersEmailService.Object, muambatorServiceMock.Object, bookRepositoryMock.Object,
            unitOfWorkMock.Object, bookUserValidator.Object, configurationMock.Object, currentUserAccessorMock.Object);


        string reason = "I need this book because I'm learning a new programming language.";

        await service.InsertAsync(bookId, reason);

        var insertedRequest = await context.BookUser.SingleOrDefaultAsync(bu => bu.BookId == bookId);
        Assert.NotNull(insertedRequest);
        Assert.Equal(reason, insertedRequest.Reason);
    }
}
