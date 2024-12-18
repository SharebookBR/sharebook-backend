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
    [InlineData(1, 1, false, 1)]
    [InlineData(1, 10, false, 8)]
    [InlineData(2, 10, false, 0)]
    [InlineData(2, 5, false, 3)]
    [InlineData(3, 3, false, 2)]
    [InlineData(1, 5, true, 1)]
    [InlineData(2, 5, true, 0)]
    public async Task MeetupList(int page, int pageSize, bool upcoming, int expectedQuantity)
    {
        var response = await _fixture.ShareBookApiClient.GetAsync($"api/Meetup?page={page}&pageSize={pageSize}&upcoming={upcoming}");

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string responseAsString = await response.Content.ReadAsStringAsync();
        responseAsString.Should().NotBeNullOrWhiteSpace();
        PagedList<Meetup>? meetups = JsonConvert.DeserializeObject<PagedList<Meetup>>(responseAsString);
        meetups.Should().NotBeNull();
        meetups!.Page.Should().Be(page);
        meetups!.ItemsPerPage.Should().Be(pageSize);
        meetups!.Items.Should().HaveCount(expectedQuantity);
        meetups!.TotalItems.Should().Be(upcoming ? 1 : 8);

        meetups!.Items.All(i =>
            !string.IsNullOrWhiteSpace(i.Title)
            && !string.IsNullOrWhiteSpace(i.Description)
            && !string.IsNullOrWhiteSpace(i.Cover)
            && i.StartDate != default
        ).Should().BeTrue();
    }

    [Theory]
    [InlineData("Qualidade de vida", 1)]
    [InlineData("Azure", 2)]
    [InlineData("invalid-nonexist", 0)]
    public async Task MeetupSearch(string criteria, int totalExpected)
    {
        var response = await _fixture.ShareBookApiClient.GetAsync($"api/Meetup/search?criteria={criteria}");

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string responseAsString = await response.Content.ReadAsStringAsync();
        responseAsString.Should().NotBeNullOrWhiteSpace();
        IList<Meetup>? meetups = JsonConvert.DeserializeObject<IList<Meetup>>(responseAsString);
        meetups.Should().NotBeNull();
        meetups!.Count.Should().Be(totalExpected);

        meetups!.All(i =>
            !string.IsNullOrWhiteSpace(i.Title)
            && i.Title.Contains(criteria, StringComparison.InvariantCultureIgnoreCase)
            && !string.IsNullOrWhiteSpace(i.Description)
            && !string.IsNullOrWhiteSpace(i.Cover)
            && i.StartDate != default
        ).Should().BeTrue();
    }
}
