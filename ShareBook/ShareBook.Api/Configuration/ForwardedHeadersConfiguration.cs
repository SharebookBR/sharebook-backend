using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ShareBook.Api.Configuration;

public static class ForwardedHeadersConfiguration
{
    private const string KnownIPNetworksSection = "ForwardedHeaders:KnownIPNetworks";

    public static IServiceCollection AddShareBookForwardedHeaders(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var knownIPNetworks = configuration
            .GetSection(KnownIPNetworksSection)
            .Get<string[]>()
            ?? [];

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor;
            options.ForwardLimit = 1;

            foreach (var network in knownIPNetworks)
                options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(network));
        });

        return services;
    }
}
