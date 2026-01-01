using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ShareBook.Api;

namespace ShareBook.Test.Integration.Setup;

public class ShareBookWebAppFactory : WebApplicationFactory<Program>
{
    protected override IHostBuilder? CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                // Add test configuration values
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["TokenConfigurations:SecretJwtKey"] = "ShareBookTestSecretKeyForIntegrationTests12345",
                    ["TokenConfigurations:Audience"] = "ShareBookAudience",
                    ["TokenConfigurations:Issuer"] = "Sharebook",
                    ["TokenConfigurations:Seconds"] = "86400",
                    ["DatabaseProvider"] = "inmemory",
                    ["Rollbar:IsActive"] = "false",
                    ["EmailSettings:IsActive"] = "false",
                    ["PushNotificationSettings:IsActive"] = "false",
                    ["Muambator:IsActive"] = "false",
                    ["AwsSqsSettings:IsActive"] = "false",
                    ["MeetupSettings:IsActive"] = "false"
                });
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        Startup.IgnoreMigrations = true;
    }
}