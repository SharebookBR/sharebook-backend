using Newtonsoft.Json;
using ShareBook.Api.ViewModels;
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
        PagedList<CategoryVM>? categories = JsonConvert.DeserializeObject<PagedList<CategoryVM>>(responseAsString);
        categories.Should().NotBeNull();
        categories!.Items.Should().NotBeNull();
        categories.Items.Count.Should().Be(11);
        categories.ItemsPerPage.Should().Be(50);
        categories.Page.Should().Be(1);

        categories.Items.All(i =>
            !string.IsNullOrWhiteSpace(i.Name)
            && i.Id != default
        ).Should().BeTrue();
        categories.Items.Any(i => i.Name == "Tecnologia" && i.Children.Count == 6).Should().BeTrue();
        categories.Items.Any(i => i.Name == "Ficção" && i.Children.Count == 6).Should().BeTrue();
    }

    [Fact]
    public async Task GetCategoryById()
    {
        var categoriesResponse = await _fixture.ShareBookApiClient.GetAsync("api/category");
        var categoriesJson = await categoriesResponse.Content.ReadAsStringAsync();
        var categories = JsonConvert.DeserializeObject<PagedList<CategoryVM>>(categoriesJson);
        var expected = categories!.Items.First();

        var response = await _fixture.ShareBookApiClient.GetAsync($"api/category/{expected.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseAsString = await response.Content.ReadAsStringAsync();
        var category = JsonConvert.DeserializeObject<CategoryVM>(responseAsString);
        category.Should().NotBeNull();
        category!.Id.Should().Be(expected.Id);
        category.Name.Should().Be(expected.Name);
    }

    [Fact]
    public async Task GetCategoriesPaged()
    {
        var response = await _fixture.ShareBookApiClient.GetAsync("api/category/1/5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseAsString = await response.Content.ReadAsStringAsync();
        var categories = JsonConvert.DeserializeObject<PagedList<CategoryVM>>(responseAsString);
        categories.Should().NotBeNull();
        categories!.Page.Should().Be(1);
        categories.ItemsPerPage.Should().Be(5);
        categories.Items.Count.Should().Be(5);
    }

    [Fact]
    public async Task GetCategoriesWithCounts()
    {
        var response = await _fixture.ShareBookApiClient.GetAsync("api/category/Counts");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var responseAsString = await response.Content.ReadAsStringAsync();
        var categories = JsonConvert.DeserializeObject<List<CategoryVM>>(responseAsString);
        categories.Should().NotBeNullOrEmpty();
    }
}
