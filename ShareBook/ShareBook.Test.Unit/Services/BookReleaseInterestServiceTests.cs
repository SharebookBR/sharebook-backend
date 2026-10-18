using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Domain.Enums;
using ShareBook.Domain.Exceptions;
using ShareBook.Repository;
using ShareBook.Service;
using ShareBook.Service.Upload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ShareBook.Test.Unit.Services;

public class BookReleaseInterestServiceTests
{
    private static readonly DateTimeOffset NowUtc = new(2026, 11, 1, 10, 0, 0, TimeSpan.Zero);

    private readonly Mock<ICurrentUserAccessor> _currentUser = new();
    private readonly Mock<IBooksEmailService> _email = new();
    private readonly Mock<IUserService> _userService = new();
    private readonly Mock<IUploadService> _upload = new();

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    public BookReleaseInterestServiceTests()
    {
        _userService.Setup(s => s.GenerateUnsubscribeToken(It.IsAny<Guid>())).Returns("tok");
        _upload.Setup(s => s.GetImageUrl(It.IsAny<string>(), "Books", It.IsAny<int>())).Returns("https://img/capa.png");
    }

    [Fact]
    public async Task Subscribe_ToScheduledBook_CountsOnceEvenWhenRepeated()
    {
        await using var context = NewContext();
        var book = await AddBookAsync(context, BookStatus.Scheduled);
        var user = await AddUserAsync(context);
        var service = CreateService(context, user.Id);

        await service.SubscribeAsync(book.Id);
        var status = await service.SubscribeAsync(book.Id);

        Assert.Equal(1, status.Total);
        Assert.True(status.Subscribed);
        Assert.Equal(1, await context.BookReleaseInterests.CountAsync());
    }

    [Fact]
    public async Task Status_ShowsTotalAcrossPeople_AndSubscribedOnlyForTheCaller()
    {
        await using var context = NewContext();
        var book = await AddBookAsync(context, BookStatus.Scheduled);
        var maria = await AddUserAsync(context);
        var joao = await AddUserAsync(context);
        await CreateService(context, maria.Id).SubscribeAsync(book.Id);
        await CreateService(context, joao.Id).SubscribeAsync(book.Id);

        var asMaria = await CreateService(context, maria.Id).GetStatusAsync(book.Id, maria.Id);
        var asVisitor = await CreateService(context, maria.Id).GetStatusAsync(book.Id, null);

        Assert.Equal(2, asMaria.Total);
        Assert.True(asMaria.Subscribed);
        Assert.Equal(2, asVisitor.Total);
        Assert.False(asVisitor.Subscribed);
    }

    [Fact]
    public async Task Unsubscribe_RemovesInterest_AndIsIdempotent()
    {
        await using var context = NewContext();
        var book = await AddBookAsync(context, BookStatus.Scheduled);
        var user = await AddUserAsync(context);
        var service = CreateService(context, user.Id);
        await service.SubscribeAsync(book.Id);

        var first = await service.UnsubscribeAsync(book.Id);
        var second = await service.UnsubscribeAsync(book.Id);

        Assert.Equal(0, first.Total);
        Assert.False(first.Subscribed);
        Assert.Equal(0, second.Total);
    }

    [Theory]
    [InlineData(BookStatus.Available, "já está liberado")]
    [InlineData(BookStatus.Canceled, "não está em pré-lançamento")]
    [InlineData(BookStatus.WaitingApproval, "não está em pré-lançamento")]
    public async Task Subscribe_RejectsBooksThatAreNotScheduled(BookStatus status, string expectedMessage)
    {
        await using var context = NewContext();
        var book = await AddBookAsync(context, status);
        var user = await AddUserAsync(context);

        var exception = await Assert.ThrowsAsync<ShareBookException>(() => CreateService(context, user.Id).SubscribeAsync(book.Id));

        Assert.Contains(expectedMessage, exception.Message);
        Assert.Empty(context.BookReleaseInterests);
    }

    [Fact]
    public async Task Subscribe_RejectsPeopleWhoDisabledEmails_WithAClearMessage()
    {
        await using var context = NewContext();
        var book = await AddBookAsync(context, BookStatus.Scheduled);
        var user = await AddUserAsync(context, allowEmail: false);

        var exception = await Assert.ThrowsAsync<ShareBookException>(() => CreateService(context, user.Id).SubscribeAsync(book.Id));

        Assert.Contains("desativou o recebimento de e-mails", exception.Message);
        Assert.Empty(context.BookReleaseInterests);
    }

    [Fact]
    public async Task Subscribe_ToUnknownBook_IsNotFound()
    {
        await using var context = NewContext();
        var user = await AddUserAsync(context);

        await Assert.ThrowsAsync<ShareBookException>(() => CreateService(context, user.Id).SubscribeAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task Notify_SendsOnlyForReleasedBooks_AndOnlyOnce()
    {
        await using var context = NewContext();
        var released = await AddBookAsync(context, BookStatus.Available);
        var stillScheduled = await AddBookAsync(context, BookStatus.Scheduled);
        var maria = await AddUserAsync(context);
        var joao = await AddUserAsync(context);
        await AddInterestAsync(context, released.Id, maria.Id);
        await AddInterestAsync(context, released.Id, joao.Id);
        await AddInterestAsync(context, stillScheduled.Id, maria.Id);
        var service = CreateService(context, maria.Id);

        var first = await service.NotifyReleasedBooksAsync();
        var second = await service.NotifyReleasedBooksAsync();

        Assert.Equal(2, first);
        Assert.Equal(0, second);
        _email.Verify(e => e.SendEmailBookReleasedAsync(It.Is<Book>(b => b.Id == released.Id), It.IsAny<User>(), "https://img/capa.png", It.IsAny<string>(), It.Is<string>(u => u.Contains("token=tok"))), Times.Exactly(2));
        _email.Verify(e => e.SendEmailBookReleasedAsync(It.Is<Book>(b => b.Id == stillScheduled.Id), It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        Assert.All(context.BookReleaseInterests.Where(i => i.BookId == released.Id), i => Assert.Equal(NowUtc.UtcDateTime, i.NotifiedAt));
        Assert.Null(context.BookReleaseInterests.Single(i => i.BookId == stillScheduled.Id).NotifiedAt);
    }

    [Fact]
    public async Task Notify_AFailedEmailStaysPending_AndDoesNotBlockTheOthers()
    {
        await using var context = NewContext();
        var book = await AddBookAsync(context, BookStatus.Available);
        var broken = await AddUserAsync(context);
        var healthy = await AddUserAsync(context);
        await AddInterestAsync(context, book.Id, broken.Id);
        await AddInterestAsync(context, book.Id, healthy.Id);
        _email.Setup(e => e.SendEmailBookReleasedAsync(It.IsAny<Book>(), It.Is<User>(u => u.Id == broken.Id), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("smtp fora do ar"));
        var service = CreateService(context, healthy.Id);

        var sent = await service.NotifyReleasedBooksAsync();

        Assert.Equal(1, sent);
        Assert.Null(context.BookReleaseInterests.Single(i => i.UserId == broken.Id).NotifiedAt);
        Assert.NotNull(context.BookReleaseInterests.Single(i => i.UserId == healthy.Id).NotifiedAt);

        // Próxima execução com o SMTP de volta: só o pendente é reenviado.
        _email.Reset();
        var retried = await service.NotifyReleasedBooksAsync();
        Assert.Equal(1, retried);
        _email.Verify(e => e.SendEmailBookReleasedAsync(It.IsAny<Book>(), It.Is<User>(u => u.Id == broken.Id), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        _email.Verify(e => e.SendEmailBookReleasedAsync(It.IsAny<Book>(), It.Is<User>(u => u.Id == healthy.Id), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Notify_SkipsPeopleWhoTurnedEmailOffSinceAsking_WithoutSending()
    {
        await using var context = NewContext();
        var book = await AddBookAsync(context, BookStatus.Available);
        var user = await AddUserAsync(context, allowEmail: false);
        await AddInterestAsync(context, book.Id, user.Id);

        var sent = await CreateService(context, user.Id).NotifyReleasedBooksAsync();

        Assert.Equal(0, sent);
        _email.Verify(e => e.SendEmailBookReleasedAsync(It.IsAny<Book>(), It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        Assert.NotNull(context.BookReleaseInterests.Single().NotifiedAt);
    }

    private static ApplicationDbContext NewContext()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private BookReleaseInterestService CreateService(ApplicationDbContext context, Guid currentUserId)
    {
        _currentUser.Setup(c => c.UserId).Returns(currentUserId);
        _currentUser.Setup(c => c.RequireUserId()).Returns(currentUserId);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ServerSettings:FrontendUrl"] = "https://www.sharebook.com.br" })
            .Build();
        return new BookReleaseInterestService(context, _currentUser.Object, _email.Object, _userService.Object, _upload.Object,
            configuration, new FixedTimeProvider(NowUtc));
    }

    private static async Task<Book> AddBookAsync(ApplicationDbContext context, BookStatus status)
    {
        var owner = new User { Name = "Quem doa", Email = $"{Guid.NewGuid()}@example.com", Password = "x", PasswordSalt = "x", Active = true };
        var category = new Category { Name = "Fantasia" };
        var book = new Book
        {
            Title = "Harry Potter", Author = "J. K. Rowling", Slug = $"hp-{Guid.NewGuid():N}", Status = status,
            Type = BookType.Printed, User = owner, Category = category, ImageSlug = "hp.png", ChooseDate = new DateTime(2026, 11, 11)
        };
        context.Books.Add(book);
        await context.SaveChangesAsync();
        return book;
    }

    private static async Task<User> AddUserAsync(ApplicationDbContext context, bool allowEmail = true)
    {
        var user = new User { Name = "Maria Silva", Email = $"{Guid.NewGuid()}@example.com", Password = "x", PasswordSalt = "x", Active = true, AllowSendingEmail = allowEmail };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    private static async Task AddInterestAsync(ApplicationDbContext context, Guid bookId, Guid userId)
    {
        context.BookReleaseInterests.Add(new BookReleaseInterest { BookId = bookId, UserId = userId });
        await context.SaveChangesAsync();
    }
}
