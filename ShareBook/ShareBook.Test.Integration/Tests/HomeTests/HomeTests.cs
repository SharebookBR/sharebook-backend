using System.Net;
using Newtonsoft.Json;
using ShareBook.Domain.DTOs;
using ShareBook.Domain.Enums;

namespace ShareBook.Test.Integration.Tests.HomeTests;

[Collection(nameof(ShareBookTestsFixture))]
public class HomeTests
{
    private readonly ShareBookTestsFixture _fixture;

    public HomeTests(ShareBookTestsFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task FeaturedPrintedBooks_ReturnsCompactAvailableList()
    {
        var response = await _fixture.ShareBookApiClient.GetAsync("api/home/featured-printed-books");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseAsString = await response.Content.ReadAsStringAsync();
        var books = JsonConvert.DeserializeObject<IList<HomeShowcaseBookDTO>>(responseAsString);

        var expectedCount = _fixture.ApplicationDbContext.Books
            .Count(b => b.Status == BookStatus.Available && b.Type == BookType.Printed);

        books.Should().NotBeNull();
        books!.Count.Should().Be(Math.Min(expectedCount, 15));
        books.Should().OnlyContain(book =>
            book.Type == BookType.Printed.ToString()
            && !string.IsNullOrWhiteSpace(book.Title)
            && !string.IsNullOrWhiteSpace(book.Slug)
            && !string.IsNullOrWhiteSpace(book.ImageUrl));
    }

    [Fact]
    public async Task BookCover_HasTemporaryBrowserCache()
    {
        var response = await _fixture.ShareBookApiClient.GetAsync(
            "Images/Books/a-cabana.jpg",
            HttpCompletionOption.ResponseHeadersRead);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.CacheControl.Should().NotBeNull();
        response.Headers.CacheControl!.Public.Should().BeTrue();
        response.Headers.CacheControl.MaxAge.Should().Be(TimeSpan.FromDays(1));
    }
}
