using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ShareBook.Domain.Common;
using ShareBook.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareBook.Api.Middleware
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (ShareBookException ex)
            {
                var result = new Result();
                result.Messages.Add(ex.Message);
                var jsonResponse = ToJson(result);

                httpContext.Response.Clear();
                httpContext.Response.StatusCode = (int)ex.ErrorType;
                httpContext.Response.Headers.TryAdd("Content-Type", "application/json");
                await httpContext.Response.WriteAsync(jsonResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");

                var result = new Result();
                result.Messages.Add(ex.ToString());

                // detalhes do erro real pra facilitar o desenvolvimento.
                if (ex is AggregateException)
                    result.Messages.Add(ex.InnerException.ToString());

                var jsonResponse = ToJson(result);

                httpContext.Response.Clear();
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                httpContext.Response.Headers.TryAdd("Content-Type", "application/json");
                await httpContext.Response.WriteAsync(jsonResponse);
            }
        }

        private string ToJson(Object obj)
        {
            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            string json = JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented
            });

            return json;
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandlerMiddleware(this IApplicationBuilder builder) => builder.UseMiddleware<ExceptionHandlerMiddleware>();
    }
}
