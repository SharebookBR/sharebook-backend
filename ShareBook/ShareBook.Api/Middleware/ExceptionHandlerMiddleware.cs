using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
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
                var jsonResponse = JsonConvert.SerializeObject(result);

                httpContext.Response.Clear();
                httpContext.Response.StatusCode = (int)ex.ErrorType;
                httpContext.Response.Headers.Add("Content-Type", "application/json");
                await httpContext.Response.WriteAsync(jsonResponse);
            }
            catch (Exception ex)
            {
                if (RollbarConfigurator.IsActive)
                {
                    SendErrorToRollbar(ex);
                }

                // página padrão de erro do core tem mais detalhes.
                throw ex;
            }
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