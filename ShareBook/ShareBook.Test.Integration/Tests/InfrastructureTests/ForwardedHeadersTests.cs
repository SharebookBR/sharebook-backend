using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShareBook.Api.Configuration;
using System.Net;

namespace ShareBook.Test.Integration.Tests.InfrastructureTests;

public class ForwardedHeadersTests
{
    [Fact]
    public async Task TrustedProxy_UsesForwardedClientIp()
    {
        var remoteIp = await ResolveRemoteIpAsync(
            proxyIp: "10.0.1.14",
            forwardedFor: "203.0.113.25");

        remoteIp.Should().Be(IPAddress.Parse("203.0.113.25"));
    }

    [Fact]
    public async Task UntrustedProxy_IgnoresForgedForwardedClientIp()
    {
        var remoteIp = await ResolveRemoteIpAsync(
            proxyIp: "192.0.2.10",
            forwardedFor: "203.0.113.25");

        remoteIp.Should().Be(IPAddress.Parse("192.0.2.10"));
    }

    [Fact]
    public async Task TrustedProxy_ProcessesOnlyTheRightmostForwardedIp()
    {
        var remoteIp = await ResolveRemoteIpAsync(
            proxyIp: "10.0.1.14",
            forwardedFor: "198.51.100.20, 203.0.113.25");

        remoteIp.Should().Be(IPAddress.Parse("203.0.113.25"));
    }

    private static async Task<IPAddress?> ResolveRemoteIpAsync(
        string proxyIp,
        string forwardedFor)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ForwardedHeaders:KnownIPNetworks:0"] = "10.0.1.0/24"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddShareBookForwardedHeaders(configuration);

        await using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider
            .GetRequiredService<IOptions<ForwardedHeadersOptions>>();
        var loggerFactory = serviceProvider
            .GetRequiredService<ILoggerFactory>();

        IPAddress? resolvedIp = null;
        var middleware = new ForwardedHeadersMiddleware(
            context =>
            {
                resolvedIp = context.Connection.RemoteIpAddress;
                return Task.CompletedTask;
            },
            loggerFactory,
            options);

        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse(proxyIp);
        httpContext.Request.Headers["X-Forwarded-For"] = forwardedFor;

        await middleware.Invoke(httpContext);
        return resolvedIp;
    }
}
