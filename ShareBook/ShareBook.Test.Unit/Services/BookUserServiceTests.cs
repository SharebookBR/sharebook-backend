using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Domain.Enums;
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
            var book = BookMock.GetLordTheRings();
            book.Id = bookId;
            return book;
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

    [Fact]
    public async Task RequestBook_WithCustomLimit_ClosesBookAtCustomLimit()
    {
        await using var context = CreateContext();
        var book = BookMock.GetLordTheRings();
        book.Id = bookId;
        book.MaxRequestsOverride = 1;
        bookServiceMock.Setup(s => s.GetBookWithAllUsersAsync(It.IsAny<Guid>())).ReturnsAsync(book);

        var service = new BookUserService(context,
            bookServiceMock.Object, bookUsersEmailService.Object, muambatorServiceMock.Object, bookRepositoryMock.Object,
            unitOfWorkMock.Object, bookUserValidator.Object, configurationMock.Object, currentUserAccessorMock.Object);

        await service.InsertAsync(bookId, "custom limit");

        Assert.Equal(BookStatus.AwaitingDonorDecision, book.Status);
        bookUsersEmailService.Verify(x => x.SendEmailMaxRequestsAsync(book), Times.Once);
    }

    [Fact]
    public async Task RequestBook_WithoutOverride_UsesFallbackLimitOf50()
    {
        await using var context = CreateContext();
        var book = BookMock.GetLordTheRings();
        book.Id = bookId;
        book.MaxRequestsOverride = null;
        bookServiceMock.Setup(s => s.GetBookWithAllUsersAsync(It.IsAny<Guid>())).ReturnsAsync(book);
        configurationMock.Setup(c => c["SharebookSettings:MaxRequestsPerBook"]).Returns((string?)null);
        await AddRequestsAsync(context, bookId, 49);

        var service = CreateService(context);

        await service.InsertAsync(bookId, "fallback limit");

        Assert.Equal(50, await context.BookUser.CountAsync(x => x.BookId == bookId));
        Assert.Equal(BookStatus.AwaitingDonorDecision, book.Status);
        bookUsersEmailService.Verify(x => x.SendEmailMaxRequestsAsync(book), Times.Once);
    }

    [Fact]
    public async Task RequestBook_WithCustomLimit_RemainsAvailableBeforeLimit()
    {
        await using var context = CreateContext();
        var book = BookMock.GetLordTheRings();
        book.Id = bookId;
        book.MaxRequestsOverride = 2;
        bookServiceMock.Setup(s => s.GetBookWithAllUsersAsync(It.IsAny<Guid>())).ReturnsAsync(book);
        await AddRequestsAsync(context, bookId, 0);

        var service = CreateService(context);

        await service.InsertAsync(bookId, "before custom limit");

        Assert.Equal(BookStatus.Available, book.Status);
        bookUsersEmailService.Verify(x => x.SendEmailMaxRequestsAsync(It.IsAny<Book>()), Times.Never);
    }

    [Theory]
    [InlineData(2, 1, true)]
    [InlineData(51, 49, false)]
    [InlineData(51, 50, true)]
    public async Task RequestBook_UsesCustomLimitBelowOrAboveGlobalDefault(int customLimit, int existingRequests, bool shouldClose)
    {
        await using var context = CreateContext();
        var book = BookMock.GetLordTheRings();
        book.Id = bookId;
        book.MaxRequestsOverride = customLimit;
        bookServiceMock.Setup(s => s.GetBookWithAllUsersAsync(It.IsAny<Guid>())).ReturnsAsync(book);
        await AddRequestsAsync(context, bookId, existingRequests);

        var service = CreateService(context);

        await service.InsertAsync(bookId, "custom limit boundary");

        Assert.Equal(existingRequests + 1, await context.BookUser.CountAsync(x => x.BookId == bookId));
        Assert.Equal(shouldClose ? BookStatus.AwaitingDonorDecision : BookStatus.Available, book.Status);
        bookUsersEmailService.Verify(
            x => x.SendEmailMaxRequestsAsync(It.IsAny<Book>()),
            shouldClose ? Times.Once() : Times.Never());
    }

    [Fact]
    public async Task RequestBook_Ebook_DoesNotUsePhysicalRequestLimit()
    {
        await using var context = CreateContext();
        var book = BookMock.GetLordTheRings();
        book.Id = bookId;
        book.Type = BookType.Eletronic;
        book.MaxRequestsOverride = 1;
        bookServiceMock.Setup(s => s.GetBookWithAllUsersAsync(It.IsAny<Guid>())).ReturnsAsync(book);
        await AddRequestsAsync(context, bookId, 1);

        var service = CreateService(context);

        await service.InsertAsync(bookId, "ebook request");

        Assert.Equal(BookStatus.Available, book.Status);
        bookUsersEmailService.Verify(x => x.SendEmailMaxRequestsAsync(It.IsAny<Book>()), Times.Never);
    }

    private BookUserService CreateService(ApplicationDbContext context)
        => new(context,
            bookServiceMock.Object, bookUsersEmailService.Object, muambatorServiceMock.Object, bookRepositoryMock.Object,
            unitOfWorkMock.Object, bookUserValidator.Object, configurationMock.Object, currentUserAccessorMock.Object);

    private static async Task AddRequestsAsync(ApplicationDbContext context, Guid requestedBookId, int count)
    {
        for (var index = 0; index < count; index++)
        {
            context.BookUser.Add(new BookUser
            {
                BookId = requestedBookId,
                UserId = Guid.NewGuid(),
                Reason = $"Existing request {index}"
            });
        }

        await context.SaveChangesAsync();
    }
}
