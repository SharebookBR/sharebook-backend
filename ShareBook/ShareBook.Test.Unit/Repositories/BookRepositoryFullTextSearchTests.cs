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
}
