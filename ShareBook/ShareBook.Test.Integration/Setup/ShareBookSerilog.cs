using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShareBook.Api;
using ShareBook.Repository;
using Serilog;

namespace ShareBook.Test.Integration.Setup
{
    public class ShareBookSerilog : WebApplicationFactory<Program>
    {
        public static InMemoryLogSink InMemorySink = new InMemoryLogSink();

        protected override IHostBuilder CreateHostBuilder()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Sink(InMemorySink)
                .CreateLogger();

            return Host.CreateDefaultBuilder()
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            Startup.IgnoreMigrations = true;

            builder.ConfigureTestServices(services =>
            {
                var dbOptions = services.FirstOrDefault(x => x.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (dbOptions != null)
                    services.Remove(dbOptions);

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("ShareBookLogsInMemoryDb");
                });
            });
        }
    }
}