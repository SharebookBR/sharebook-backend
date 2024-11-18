using System.Net;
using Newtonsoft.Json;
using ShareBook.Domain;
using ShareBook.Domain.Common;

namespace ShareBook.Test.Integration.Tests.MeetupTests;

[Collection(nameof(ShareBookTestsFixture))]
public class MeetupTests
{
    private readonly ShareBookTestsFixture _fixture;

    public MeetupTests(ShareBookTestsFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineData(1, 1, 1)]
    [InlineData(1, 10, 8)]
    [InlineData(2, 10, 0)]
    [InlineData(2, 5, 3)]
    [InlineData(3, 3, 2)]
    public async Task MeetupList(int page, int pageSize, int expectedQuantity)
    {
        // TODO: Add tests for upcoming=true
        var response = await _fixture.ShareBookApiClient.GetAsync($"api/Meetup?page={page}&pageSize={pageSize}");

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string responseAsString = await response.Content.ReadAsStringAsync();
        responseAsString.Should().NotBeNullOrWhiteSpace();
        PagedList<Meetup>? meetups = JsonConvert.DeserializeObject<PagedList<Meetup>>(responseAsString);
        meetups.Should().NotBeNull();
        meetups!.Page.Should().Be(page);
        meetups!.ItemsPerPage.Should().Be(pageSize);
        meetups!.Items.Should().HaveCount(expectedQuantity);
        // TODO: Validate all items have title, url and so on
        meetups!.TotalItems.Should().Be(8);
    }
}
