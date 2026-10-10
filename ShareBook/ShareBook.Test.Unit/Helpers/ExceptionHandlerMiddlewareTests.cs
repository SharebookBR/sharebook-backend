using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using ShareBook.Api.Middleware;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace ShareBook.Test.Unit.Helpers;

public class ExceptionHandlerMiddlewareTests
{
    [Fact]
    public async Task BadHttpRequest_ReturnsClientStatusInsteadOf500()
    {
        var middleware = new ExceptionHandlerMiddleware(
            _ => throw CreateBadHttpRequest(StatusCodes.Status408RequestTimeout),
            NullLogger<ExceptionHandlerMiddleware>.Instance);
        var context = new DefaultHttpContext();

        await middleware.Invoke(context);

        Assert.Equal(StatusCodes.Status408RequestTimeout, context.Response.StatusCode);
    }

    [Fact]
    public async Task UnhandledException_StillReturns500()
    {
        var middleware = new ExceptionHandlerMiddleware(
            _ => throw new InvalidOperationException("unexpected"),
            NullLogger<ExceptionHandlerMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new System.IO.MemoryStream();

        await middleware.Invoke(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
    }

    // BadHttpRequestException tem construtor internal/protected conforme a versao; usa reflection.
    private static BadHttpRequestException CreateBadHttpRequest(int statusCode)
    {
        return (BadHttpRequestException)Activator.CreateInstance(
            typeof(BadHttpRequestException),
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            [ "Reading the request body timed out", statusCode ],
            null)!;
    }
}
