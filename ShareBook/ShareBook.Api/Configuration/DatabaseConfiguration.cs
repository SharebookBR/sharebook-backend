using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShareBook.Repository;
using System;

namespace ShareBook.Api.Configuration;

public static class DatabaseConfiguration
{
    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration config)
    {
        var dbProvider = config["DatabaseProvider"]?.ToLower() ?? "sqlserver";

        // Health Checks - skip for inmemory (tests)
        if (dbProvider != "inmemory")
        {
            var healthChecksBuilder = services.AddHealthChecks();

            switch (dbProvider)
            {
                case "postgres":
                    healthChecksBuilder.AddNpgSql(config.GetConnectionString("PostgresConnection"));
                    break;

                case "sqlite":
                    healthChecksBuilder.AddSqlite(config.GetConnectionString("SqliteConnection"));
                    break;

                default:
                    healthChecksBuilder.AddSqlServer(config.GetConnectionString("DefaultConnection"));
                    break;
            }
        }
        else
        {
            services.AddHealthChecks();
        }

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            switch (dbProvider)
            {
                case "postgres":
                    options.UseNpgsql(config.GetConnectionString("PostgresConnection"));
                    break;

                case "sqlite":
                    options.UseSqlite(config.GetConnectionString("SqliteConnection"));
                    break;

                case "inmemory":
                    options.UseInMemoryDatabase("ShareBookInMemoryDb");
                    break;

                default:
                    options.UseSqlServer(config.GetConnectionString("DefaultConnection"));
                    break;
            }
        });

        return services;
    }

    public static void EnsureDatabaseCreated(IServiceProvider serviceProvider, IConfiguration config)
    {
        var dbProvider = config["DatabaseProvider"]?.ToLower() ?? "sqlserver";

        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (dbProvider == "sqlite" || dbProvider == "inmemory")
            context.Database.EnsureCreated();
        else
            context.Database.Migrate();
    }
}