using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShareBook.Api;
using ShareBook.Repository;

namespace ShareBook.Test.Integration.Setup;

public class ShareBookWebAppFactory : WebApplicationFactory<Program>
{
    protected override IHostBuilder? CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        Startup.IgnoreMigrations = true;

        builder.ConfigureTestServices(services =>
        {
            var dbOptions = services.FirstOrDefault(x => x.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (dbOptions != null)
                services.Remove(dbOptions);

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("ShareBookInMemoryDb");
            });
        });
    }
}