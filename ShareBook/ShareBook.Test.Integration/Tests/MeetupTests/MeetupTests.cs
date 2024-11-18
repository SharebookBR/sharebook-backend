using System.Net;
using System.Text.Json;
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

    [Fact]
    public async Task MeetupList_Empty()
    {
        var response = await _fixture.ShareBookApiClient.GetAsync("api/Meetup");

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string responseAsString = await response.Content.ReadAsStringAsync();
        responseAsString.Should().NotBeNullOrWhiteSpace();
        PagedList<Meetup>? meetups = JsonSerializer.Deserialize<PagedList<Meetup>>(responseAsString);
        meetups.Should().NotBeNull();
        meetups!.Page.Should().Be(0);
        meetups!.Items.Should().BeNull();
    }
}
