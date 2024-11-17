using Microsoft.Extensions.DependencyInjection;
using ShareBook.Repository;

namespace ShareBook.Test.Integration.Setup;


[CollectionDefinition(nameof(ShareBookTestsFixture))]
public class ShareBookTestsFixtureCollection : ICollectionFixture<ShareBookTestsFixture>
{ }

public class ShareBookTestsFixture
{
    public ShareBookWebAppFactory ShareBookWebAppFactory { get; } = new ShareBookWebAppFactory();
    public HttpClient ShareBookApiClient { get; init; }
    public ApplicationDbContext ApplicationDbContext { get; init; }

    public ShareBookTestsFixture()
    {
        ShareBookApiClient = ShareBookWebAppFactory.CreateClient();
        ApplicationDbContext = ShareBookWebAppFactory.Services.GetRequiredService<ApplicationDbContext>();
    }
}
