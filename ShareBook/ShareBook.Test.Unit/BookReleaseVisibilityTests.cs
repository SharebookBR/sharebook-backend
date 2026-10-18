using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Repository;
using Xunit;

namespace ShareBook.Test.Unit;

public class BookReleaseVisibilityTests
{
    private static readonly DateTime Now = new(2026, 11, 1, 15, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(BookStatus.Available, BookType.Printed, null, true)]
    [InlineData(BookStatus.Available, BookType.Printed, -1, true)]
    [InlineData(BookStatus.Available, BookType.Printed, 0, true)]
    [InlineData(BookStatus.Available, BookType.Printed, 1, false)]
    [InlineData(BookStatus.Available, BookType.Eletronic, null, true)]
    [InlineData(BookStatus.Available, BookType.Eletronic, 1, true)]
    [InlineData(BookStatus.WaitingApproval, BookType.Printed, null, false)]
    [InlineData(BookStatus.AwaitingDonorDecision, BookType.Printed, -1, false)]
    public void IsPubliclyAvailable_FollowsReleaseInstant(BookStatus status, BookType type, int? releaseOffsetSeconds, bool expected)
    {
        var book = new Book
        {
            Status = status,
            Type = type,
            ReleaseAtUtc = releaseOffsetSeconds.HasValue ? Now.AddSeconds(releaseOffsetSeconds.Value) : null
        };

        Assert.Equal(expected, BookReleasePolicy.IsPubliclyAvailable(Now).Compile()(book));
        Assert.Equal(expected ? 1 : 0, new[] { book }.AsQueryable().WherePubliclyAvailable(Now).Count());
    }

    [Fact]
    public void PublicSearch_ExcludesUpcomingBooks_ButAdminSearchDoesNot()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=sharebook_release_visibility;Username=test;Password=test")
            .Options;
        using var context = new ApplicationDbContext(options);
        var repository = new BookRepository(context);

        var publicSql = repository.FullTextSearch("harry potter", includeUnavailable: false).ToQueryString();
        var adminSql = repository.FullTextSearch("harry potter", includeUnavailable: true).ToQueryString();

        Assert.Contains("ReleaseAtUtc\" IS NULL", publicSql);
        Assert.DoesNotContain("ReleaseAtUtc\" IS NULL", adminSql);
    }

    [Fact]
    public void PubliclyAvailableFilter_TranslatesToSql()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=sharebook_release_visibility;Username=test;Password=test")
            .Options;
        using var context = new ApplicationDbContext(options);

        var sql = context.Books.WherePubliclyAvailable(Now).ToQueryString();

        Assert.Contains("ReleaseAtUtc", sql);
        Assert.Contains("Status", sql);
    }
}
