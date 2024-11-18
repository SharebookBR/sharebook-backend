using System.Net;
using System.Text.Json;
using ShareBook.Api.ViewModels;

namespace ShareBook.Test.Integration.Tests.BookTests;

[Collection(nameof(ShareBookTestsFixture))]
public class BookTests
{
    private readonly ShareBookTestsFixture _fixture;

    public BookTests(ShareBookTestsFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AvailableBooks_All()
    {
        var response = await _fixture.ShareBookApiClient.GetAsync("api/book/AvailableBooks");

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string responseAsString = await response.Content.ReadAsStringAsync();
        responseAsString.Should().NotBeNullOrWhiteSpace();
        List<BookVM>? books = JsonSerializer.Deserialize<List<BookVM>>(responseAsString);
        books.Should().NotBeNullOrEmpty();
        books!.Count.Should().Be(22);
        // TODO: Validate all items have title, author and so on
    }
}
