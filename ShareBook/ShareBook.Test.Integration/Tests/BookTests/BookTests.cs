using System.Net;

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
    public async Task AvailableBooks_Empty()
    {
        var response = await _fixture.ShareBookApiClient.GetAsync("api/book/AvailableBooks");

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string responseAsString = await response.Content.ReadAsStringAsync();
        responseAsString.Should().Be("[]");
    }
}
