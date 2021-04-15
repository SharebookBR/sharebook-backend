using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Rollbar;
using ShareBook.Api.Services;
using ShareBook.Domain.Common;
using ShareBook.Domain.Exceptions;

namespace ShareBook.Api.Middleware
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
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
                httpContext.Response.Headers.Add("Content-Type", "application/json");
                await httpContext.Response.WriteAsync(jsonResponse);
            }
            catch (Exception ex)
            {
                if (RollbarConfigurator.IsActive)
                    SendErrorToRollbar(ex);

                var result = new Result();
                result.Messages.Add(ex.ToString());

                // detalhes do erro real pra facilitar o desenvolvimento.
                if (ex is AggregateException)
                    result.Messages.Add(ex.InnerException.ToString());

                var jsonResponse = ToJson(result);

                httpContext.Response.Clear();
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                httpContext.Response.Headers.Add("Content-Type", "application/json");
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

        private void SendErrorToRollbar(Exception ex)
        {
            object error = new
            {
                Message = ex.Message,
                StackTrace = ex.StackTrace,
                Source = ex.Source
            };

            RollbarLocator.RollbarInstance.Error(error);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandlerMiddleware(this IApplicationBuilder builder) => builder.UseMiddleware<ExceptionHandlerMiddleware>();
    }

}