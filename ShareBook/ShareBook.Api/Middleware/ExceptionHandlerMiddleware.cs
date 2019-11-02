using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Rollbar;
using ShareBook.Domain.Common;
using ShareBook.Domain.Exceptions;

namespace ShareBook.Api.Middleware {
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class ExceptionHandlerMiddleware {
        private readonly RequestDelegate _next;

        public ExceptionHandlerMiddleware (RequestDelegate next) {
            _next = next;
        }

        public async Task Invoke (HttpContext httpContext) {
            try {
                await _next(httpContext);
            } catch (ShareBookException ex) {

                var result = new Result();
                result.Messages.Add(ex.Message);
                var jsonResponse = JsonConvert.SerializeObject(result);

                httpContext.Response.Clear();
                httpContext.Response.StatusCode = (int)ex.ErrorType;
                await httpContext.Response.WriteAsync(jsonResponse);

                RollbarLocator.RollbarInstance.Log(ErrorLevel.Error, jsonResponse);

                return;
            } catch (Exception ex) {
                object error = new 
                {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    Source = ex.Source
                };
                RollbarLocator.RollbarInstance.Log(ErrorLevel.Critical, error);
                throw ex;
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ExceptionHandlerMiddlewareExtensions {
        public static IApplicationBuilder UseExceptionHandlerMiddleware (this IApplicationBuilder builder) => builder.UseMiddleware<ExceptionHandlerMiddleware> ();
    }
}