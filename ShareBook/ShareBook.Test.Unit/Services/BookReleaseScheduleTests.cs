using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Domain.Enums;
using ShareBook.Domain.Exceptions;
using ShareBook.Domain.Validators;
using ShareBook.Repository;
using ShareBook.Repository.UoW;
using ShareBook.Service;
using ShareBook.Service.AwsSqs;
using ShareBook.Service.EBook;
using ShareBook.Service.Upload;
using ShareBook.Test.Unit.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ShareBook.Test.Unit.Services;

public class BookReleaseScheduleTests
{
    // 2026-11-01 10:00 UTC = 07:00 em São Paulo, mesmo dia.
    private static readonly DateTimeOffset NowUtc = new(2026, 11, 1, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Today = new(2026, 11, 1);

    private readonly Mock<IBooksEmailService> emailMock = new();

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    [Fact]
    public async Task Approve_WithFutureReleaseDate_SchedulesBookWithoutNotifying()
    {
        await using var context = NewContext();
        var book = await AddBookAsync(context, BookStatus.WaitingApproval);
        var service = CreateService(context);

        await service.ApproveAsync(book.Id, null, Today.AddDays(14));

        var saved = await context.Books.FindAsync(book.Id);
        Assert.Equal(BookStatus.Scheduled, saved!.Status);
        Assert.Equal(Today.AddDays(14), saved.ReleaseDate);
        Assert.Null(saved.ChooseDate);
        emailMock.Verify(x => x.SendEmailBookApprovedAsync(It.IsAny<Book>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-3)]
    public async Task Approve_WithoutFutureReleaseDate_BehavesAsBefore(int? offsetDays)
    {
        await using var context = NewContext();
        var book = await AddBookAsync(context, BookStatus.WaitingApproval);
        var service = CreateService(context);

        await service.ApproveAsync(book.Id, null, offsetDays.HasValue ? Today.AddDays(offsetDays.Value) : null);

        var saved = await context.Books.FindAsync(book.Id);
        Assert.Equal(BookStatus.Available, saved!.Status);
        Assert.Null(saved.ReleaseDate);
        Assert.NotNull(saved.ChooseDate);
        emailMock.Verify(x => x.SendEmailBookApprovedAsync(It.IsAny<Book>()), Times.Once);
    }

    [Fact]
    public async Task Approve_EbookWithReleaseDate_IsRejected()
    {
        await using var context = NewContext();
        var book = await AddBookAsync(context, BookStatus.WaitingApproval, BookType.Eletronic);
        var service = CreateService(context);

        await Assert.ThrowsAsync<ShareBookException>(() => service.ApproveAsync(book.Id, null, Today.AddDays(1)));

        Assert.Equal(BookStatus.WaitingApproval, (await context.Books.FindAsync(book.Id))!.Status);
    }

    [Fact]
    public async Task ReleaseDue_PromotesOnlyBooksWhoseDateArrived()
    {
        await using var context = NewContext();
        var dueToday = await AddBookAsync(context, BookStatus.Scheduled, releaseDate: Today);
        var overdue = await AddBookAsync(context, BookStatus.Scheduled, releaseDate: Today.AddDays(-2));
        var future = await AddBookAsync(context, BookStatus.Scheduled, releaseDate: Today.AddDays(1));
        var alreadyAvailable = await AddBookAsync(context, BookStatus.Available);
        var service = CreateService(context);

        var released = await service.ReleaseDueScheduledBooksAsync();

        Assert.Equal(new[] { dueToday.Id, overdue.Id }.OrderBy(x => x), released.Select(b => b.Id).OrderBy(x => x));
        foreach (var id in new[] { dueToday.Id, overdue.Id })
        {
            var saved = (await context.Books.FindAsync(id))!;
            Assert.Equal(BookStatus.Available, saved.Status);
            Assert.Null(saved.ReleaseDate);
            Assert.Equal(NowUtc.UtcDateTime, saved.ApprovedAt);
            Assert.NotNull(saved.ChooseDate);
        }
        var stillScheduled = (await context.Books.FindAsync(future.Id))!;
        Assert.Equal(BookStatus.Scheduled, stillScheduled.Status);
        Assert.Equal(Today.AddDays(1), stillScheduled.ReleaseDate);
        Assert.Equal(BookStatus.Available, (await context.Books.FindAsync(alreadyAvailable.Id))!.Status);
        emailMock.Verify(x => x.SendEmailBookApprovedAsync(It.IsAny<Book>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ReleaseDue_UsesSaoPauloDate_NotUtcDate()
    {
        // 01:00 UTC de 02/11 ainda é 22:00 de 01/11 em São Paulo: livro de 02/11 não libera.
        await using var context = NewContext();
        var book = await AddBookAsync(context, BookStatus.Scheduled, releaseDate: new DateOnly(2026, 11, 2));
        var service = CreateService(context, new DateTimeOffset(2026, 11, 2, 1, 0, 0, TimeSpan.Zero));

        var released = await service.ReleaseDueScheduledBooksAsync();

        Assert.Empty(released);
        Assert.Equal(BookStatus.Scheduled, (await context.Books.FindAsync(book.Id))!.Status);
    }

    [Fact]
    public async Task CategoryTree_ListsPhysicalAvailableThenUpcomingThenDigital_AndIgnoresOtherStatuses()
    {
        await using var context = NewContext();
        var parent = new Category { Id = Guid.NewGuid(), Name = "Ficção" };
        var leaf = new Category { Id = Guid.NewGuid(), Name = "Bruxas & Magia", ParentCategoryId = parent.Id };
        context.Categories.AddRange(parent, leaf);
        var owner = new User { Name = "Quem doa", Email = "doa@example.com", Password = "x", PasswordSalt = "x" };

        Book NewBook(string title, BookStatus status, DateTime created, DateOnly? release = null) => new()
        {
            Title = title, Author = "A", Slug = title.ToLower().Replace(' ', '-'), Status = status, Type = BookType.Printed,
            User = owner, CategoryId = leaf.Id, ImageSlug = "x.png", CreationDate = created, ReleaseDate = release
        };

        var digital = NewBook("Digital novissimo", BookStatus.Available, new DateTime(2026, 10, 10));
        digital.Type = BookType.Eletronic;

        context.Books.AddRange(
            digital,
            NewBook("Disponivel antigo", BookStatus.Available, new DateTime(2026, 1, 1)),
            NewBook("Em breve distante", BookStatus.Scheduled, new DateTime(2026, 10, 3), Today.AddDays(28)),
            NewBook("Disponivel novo", BookStatus.Available, new DateTime(2026, 10, 1)),
            NewBook("Em breve proximo", BookStatus.Scheduled, new DateTime(2026, 10, 2), Today.AddDays(14)),
            NewBook("Ja doado", BookStatus.Sent, new DateTime(2026, 9, 1)),
            NewBook("Aguardando aprovacao", BookStatus.WaitingApproval, new DateTime(2026, 10, 4)),
            NewBook("Cancelado", BookStatus.Canceled, new DateTime(2026, 10, 5)));
        await context.SaveChangesAsync();
        var service = CreateService(context);

        foreach (var result in new[] { await service.ByCategoryTreeIdAsync(parent.Id, 1, 20), await service.ByCategoryIdAsync(leaf.Id, 1, 20) })
        {
            Assert.Equal(
                new[] { "Disponivel novo", "Disponivel antigo", "Em breve proximo", "Em breve distante", "Digital novissimo" },
                result.Items.Select(b => b.Title).ToArray());
            Assert.Equal(5, result.TotalItems);
            Assert.Equal(4, result.PhysicalBooksCount);
            Assert.Equal(1, result.EbooksCount);
            Assert.Equal(Today.AddDays(14), result.Items[2].ReleaseDate);
        }
    }

    [Fact]
    public async Task TagBooks_ListsPhysicalAvailableThenUpcomingThenDigital_AndCountsAll()
    {
        await using var context = NewContext();
        var tag = new Tag { Id = "bruxas", Name = "Bruxas", Family = "tema", Status = TagStatus.Active, IsPublic = true };
        context.Tags.Add(tag);
        var category = new Category { Id = Guid.NewGuid(), Name = "Fantasia" };
        var owner = new User { Name = "Quem doa", Email = "doa@example.com", Password = "x", PasswordSalt = "x" };

        Book Tagged(string title, BookStatus status, DateOnly? release = null) => new()
        {
            Title = title, Author = "A", Slug = title.ToLower().Replace(' ', '-'), Status = status, Type = BookType.Printed,
            User = owner, Category = category, ImageSlug = "x.png", ReleaseDate = release,
            BookTags = { new BookTag { TagId = tag.Id, Position = 1, ReviewStatus = BookTagReviewStatus.Approved } }
        };

        var digitalTagged = Tagged("Digital", BookStatus.Available);
        digitalTagged.Type = BookType.Eletronic;
        digitalTagged.CreationDate = new DateTime(2026, 10, 20);

        context.Books.AddRange(
            digitalTagged,
            Tagged("Em breve", BookStatus.Scheduled, Today.AddDays(14)),
            Tagged("Disponivel", BookStatus.Available),
            Tagged("Ja doado", BookStatus.Sent));
        await context.SaveChangesAsync();
        var upload = new Mock<IUploadService>();
        var service = new TagService(context, upload.Object);

        var page = await service.GetPublicBooksAsync("bruxas", 1, 20);
        var counts = await service.GetPublicBookCountsByTagAsync(new[] { "bruxas" });

        Assert.Equal(new[] { "Disponivel", "Em breve", "Digital" }, page.Items.Select(b => b.Title).ToArray());
        Assert.Equal(3, page.TotalItems);
        Assert.Equal(3, counts["bruxas"]);
    }

    private static ApplicationDbContext NewContext()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static async Task<Book> AddBookAsync(ApplicationDbContext context, BookStatus status, BookType type = BookType.Printed, DateOnly? releaseDate = null)
    {
        var book = BookMock.GetLordTheRings();
        book.Id = Guid.NewGuid();
        book.Status = status;
        book.Type = type;
        book.ReleaseDate = releaseDate;
        book.ChooseDate = null;
        book.Category = new Category { Id = Guid.NewGuid(), Name = "Fantasia" };
        book.CategoryId = book.Category.Id;
        context.Books.Add(book);
        await context.SaveChangesAsync();
        return book;
    }

    private BookService CreateService(ApplicationDbContext context, DateTimeOffset? now = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["SharebookSettings:DaysInShowcase"] = "10" })
            .Build();
        var currentUser = new Mock<ICurrentUserAccessor>();
        return new BookService(new BookRepository(context), context,
            new Mock<IUnitOfWork>().Object, new BookValidator(),
            new Mock<IUploadService>().Object, emailMock.Object, configuration,
            new Mock<NewBookQueue>().Object, new Mock<IEBookService>().Object, new Mock<ICategoryRepository>().Object,
            new FixedTimeProvider(now ?? NowUtc), currentUser.Object, new Mock<ITagService>().Object);
    }
}
