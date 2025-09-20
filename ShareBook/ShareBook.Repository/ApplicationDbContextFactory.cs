using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ShareBook.Repository;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        var dbProvider = configuration["DatabaseProvider"]?.ToLower() ?? "sqlserver";

        switch (dbProvider)
        {
            case "postgres":
                optionsBuilder.UseNpgsql(configuration.GetConnectionString("PostgresConnection"));
                break;

            case "sqlite":
                optionsBuilder.UseSqlite(configuration.GetConnectionString("SqliteConnection"));
                break;

            default:
                optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
                break;
        }

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}