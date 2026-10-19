using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Repository;
using ShareBook.Service.Analytics;
using System.Threading.Tasks;
using Xunit;

namespace ShareBook.Test.Unit.Services;

public class AnalyticsBookCatalogMetadataServiceTests
{
    [Fact]
    public async Task GetBySlugAsync_ReturnsCurrentTypeAndScheduledStatusInOneBatch()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connection));
        await using var provider = services.BuildServiceProvider();

        await using (var scope = provider.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync();
            var category = new Category { Name = "Analytics" };
            context.Books.AddRange(
                new Book { Slug = "physical-upcoming", ImageSlug = "physical-upcoming.jpg", Type = BookType.Printed, Status = BookStatus.Scheduled, Category = category },
                new Book { Slug = "digital", ImageSlug = "digital.jpg", Type = BookType.Eletronic, Status = BookStatus.Available, Category = category });
            await context.SaveChangesAsync();
        }

        var service = new AnalyticsBookCatalogMetadataService(provider.GetRequiredService<IServiceScopeFactory>());
        var result = await service.GetBySlugAsync(new[] { "physical-upcoming", "digital", "missing", "physical-upcoming" });

        Assert.Equal(2, result.Count);
        Assert.Equal(BookType.Printed, result["physical-upcoming"].Type);
        Assert.Equal(BookStatus.Scheduled, result["physical-upcoming"].Status);
        Assert.Equal(BookType.Eletronic, result["digital"].Type);
    }
}
