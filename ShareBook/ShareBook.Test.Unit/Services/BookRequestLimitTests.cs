using Microsoft.Extensions.Configuration;
using Moq;
using ShareBook.Domain;
using ShareBook.Service;
using Xunit;

namespace ShareBook.Test.Unit.Services;

public class BookRequestLimitTests
{
    [Fact]
    public void GetEffective_PrefersBookOverride()
    {
        var book = new Book { MaxRequestsOverride = 12 };
        var configuration = new Mock<IConfiguration>();
        configuration.Setup(c => c["SharebookSettings:MaxRequestsPerBook"]).Returns("50");

        Assert.Equal(12, BookRequestLimit.GetEffective(book, configuration.Object));
    }

    [Fact]
    public void GetEffective_UsesConfiguredGlobalLimitWhenOverrideIsAbsent()
    {
        var book = new Book();
        var configuration = new Mock<IConfiguration>();
        configuration.Setup(c => c["SharebookSettings:MaxRequestsPerBook"]).Returns("37");

        Assert.Equal(37, BookRequestLimit.GetEffective(book, configuration.Object));
    }
}
