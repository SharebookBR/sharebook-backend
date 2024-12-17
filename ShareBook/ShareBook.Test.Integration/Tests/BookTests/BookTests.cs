using System.Net;
using Newtonsoft.Json;
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
        IList<BookVM>? books = JsonConvert.DeserializeObject<IList<BookVM>>(responseAsString);
        books.Should().NotBeNullOrEmpty();
        books!.Count.Should().Be(22);

        books!.All(i =>
            !string.IsNullOrWhiteSpace(i.Title)
            && !string.IsNullOrWhiteSpace(i.Author)
            && !string.IsNullOrWhiteSpace(i.Slug)
            && i.CategoryId != default
        ).Should().BeTrue();
    }
}
