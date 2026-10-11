using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Serilog.Context;
using System.Threading.Tasks;

namespace ShareBook.Api.Middleware;

// Poe IP e user-agent do cliente no LogContext, entao todo log da requisicao (inclusive
// o request log e as excecoes) sai com eles. Deve rodar depois do UseForwardedHeaders
// (IP real atras do proxy) e antes do UseSerilogRequestLogging.
public class ClientIpLogContextMiddleware(RequestDelegate next)
{
    private const int MaxUserAgentLength = 200;

    public async Task Invoke(HttpContext httpContext)
    {
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        if (userAgent.Length > MaxUserAgentLength)
            userAgent = userAgent[..MaxUserAgentLength];

        using (LogContext.PushProperty("ClientIp", httpContext.Connection.RemoteIpAddress?.ToString()))
        using (LogContext.PushProperty("UserAgent", userAgent))
        {
            await next(httpContext);
        }
    }
}

public static class ClientIpLogContextMiddlewareExtensions
{
    public static IApplicationBuilder UseClientIpLogContext(this IApplicationBuilder builder) => builder.UseMiddleware<ClientIpLogContextMiddleware>();
}
