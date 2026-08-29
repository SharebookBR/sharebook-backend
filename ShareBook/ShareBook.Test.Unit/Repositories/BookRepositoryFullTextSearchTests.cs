using Microsoft.EntityFrameworkCore;
using ShareBook.Repository;
using Xunit;

namespace ShareBook.Test.Unit.Repositories;

public class BookRepositoryFullTextSearchTests
{
    [Fact]
    public void FullTextSearch_PostgresQuery_ShouldUseWeightedUnaccentedRanking()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=sharebook_search_translation;Username=test;Password=test")
            .Options;
        using var context = new ApplicationDbContext(options);
        var repository = new BookRepository(context);

        var sql = repository
            .FullTextSearch("machine learning", includeUnavailable: false)
            .ToQueryString();

        Assert.Contains("to_tsvector('simple'", sql);
        Assert.Contains("to_tsquery('simple'", sql);
        Assert.Contains("machine:* & learning:*", sql);
        Assert.Contains("unaccent(", sql);
        Assert.Contains("setweight(", sql);
        Assert.Contains("ts_rank_cd(", sql);
        Assert.Contains("replace(", sql);
        Assert.Contains("csharp", sql);
        Assert.Contains("dotnet", sql);
        Assert.Contains("ORDER BY", sql);
        Assert.Contains("Status", sql);
    }

    [Theory]
    [InlineData("fisico")]
    [InlineData("impresso")]
    [InlineData("ebook")]
    [InlineData("digital")]
    public void FullTextSearch_FormatOnly_ShouldUseStructuredBookTypeFilter(string criteria)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=sharebook_search_translation;Username=test;Password=test")
            .Options;
        using var context = new ApplicationDbContext(options);
        var repository = new BookRepository(context);

        var sql = repository
            .FullTextSearch(criteria, includeUnavailable: false)
            .ToQueryString();

        Assert.Contains("Type", sql);
        Assert.Contains("ORDER BY", sql);
        Assert.DoesNotContain("to_tsquery", sql);
    }

    [Fact]
    public void FullTextSearch_FormatAndText_ShouldFilterThenRankLexically()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=sharebook_search_translation;Username=test;Password=test")
            .Options;
        using var context = new ApplicationDbContext(options);
        var repository = new BookRepository(context);

        var sql = repository
            .FullTextSearch("ebook python", includeUnavailable: false)
            .ToQueryString();

        Assert.Contains("Type", sql);
        Assert.Contains("python:*", sql);
        Assert.Contains("to_tsquery('simple'", sql);
    }

    [Fact]
    public void FullTextSearch_ExactTitle_ShouldKeepShortWordsOutOfTsQueryButInBoost()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=sharebook_search_translation;Username=test;Password=test")
            .Options;
        using var context = new ApplicationDbContext(options);
        var repository = new BookRepository(context);

        var sql = repository
            .FullTextSearch("a divina comedia", includeUnavailable: false)
            .ToQueryString();

        Assert.Contains("divina:* & comedia:*", sql);
        Assert.DoesNotContain("'a:*", sql);
        Assert.Contains("a divina comedia", sql);
    }
}
