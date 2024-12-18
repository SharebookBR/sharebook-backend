using Newtonsoft.Json;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using System.Net;

namespace ShareBook.Test.Integration.Tests.CategoryTests;

[Collection(nameof(ShareBookTestsFixture))]
public class CategoryTests
{
    private readonly ShareBookTestsFixture _fixture;

    public CategoryTests(ShareBookTestsFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetCategories()
    {
        var response = await _fixture.ShareBookApiClient.GetAsync("api/category");

        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string responseAsString = await response.Content.ReadAsStringAsync();
        responseAsString.Should().NotBeNullOrWhiteSpace();
        PagedList<Category>? categories = JsonConvert.DeserializeObject<PagedList<Category>>(responseAsString);
        categories.Should().NotBeNull();
        categories!.Items.Should().NotBeNull();
        categories.Items.Count.Should().Be(10);
        categories.ItemsPerPage.Should().Be(50);
        categories.Page.Should().Be(1);

        categories.Items.All(i =>
            !string.IsNullOrWhiteSpace(i.Name)
            && i.Id != default
        ).Should().BeTrue();
    }
}
