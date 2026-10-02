using Microsoft.EntityFrameworkCore;
using ShareBook.Repository;
using Xunit;

namespace ShareBook.Test.Unit.Repositories;

public class TagMappingTests
{
    [Fact]
    public void CreateScript_ShouldIncludeTagsAndBookTagsConstraints()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=sharebook_tags_mapping;Username=test;Password=test")
            .Options;

        using var context = new ApplicationDbContext(options);

        var sql = context.Database.GenerateCreateScript();

        Assert.Contains("CREATE TABLE \"Tags\"", sql);
        Assert.Contains("\"Id\" character varying(100) NOT NULL", sql);
        Assert.Contains("\"Aliases\" text[] NOT NULL", sql);
        Assert.Contains("CREATE TABLE \"BookTags\"", sql);
        Assert.Contains("\"TagId\" character varying(100) NOT NULL", sql);
        Assert.Contains("CONSTRAINT \"CK_BookTags_Position\" CHECK (\"Position\" BETWEEN 1 AND 3)", sql);
        Assert.Contains("CREATE UNIQUE INDEX \"IX_BookTags_BookId_Position\"", sql);
        Assert.Contains("CREATE UNIQUE INDEX \"IX_BookTags_BookId_TagId\"", sql);
    }
}
