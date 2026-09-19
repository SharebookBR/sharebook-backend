using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShareBook.Api.RateLimiting;
using System;
using System.Net;

namespace ShareBook.Api.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ThrottleAttribute : ActionFilterAttribute
    {
        public string Name { get; set; }
        public int Seconds { get; set; }
        public string Message { get; set; }
        public bool VaryByIp { get; set; }

        // Só marca no radar da tabela "Logs" quando fizer sentido pro chamador (hoje: download de ebook).
        // Endpoints genéricos (ex.: JobExecutor) não devem virar ruído nessa tabela.
        public bool LogBlockedAttempts { get; set; }

        private static MemoryCache Cache { get; } = new MemoryCache(new MemoryCacheOptions());

        public override void OnActionExecuting(ActionExecutingContext c)
        {
            var ip = c.HttpContext.Request.HttpContext.Connection.RemoteIpAddress;
            var key = VaryByIp
            ? string.Concat(Name, "-", ip)
            : Name;

            if (!Cache.TryGetValue(key, out bool entry))
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(Seconds));

                Cache.Set(key, true, cacheEntryOptions);
            }
            else
            {
                if (string.IsNullOrEmpty(Message))
                    Message = "You may only perform this action every {n} seconds.";

                c.Result = new ContentResult { Content = Message.Replace("{n}", Seconds.ToString()) };
                c.HttpContext.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                c.HttpContext.Response.Headers["Retry-After"] = Seconds.ToString();

                if (LogBlockedAttempts)
                {
                    // Slug já foi resolvido pelo model binding antes do filtro rodar — disponível
                    // mesmo aqui, antes da action em si executar.
                    var slug = c.ActionArguments.TryGetValue("slug", out var slugValue)
                        ? slugValue?.ToString()
                        : null;

                    var logger = c.HttpContext.RequestServices.GetService<ILogger<ThrottleAttribute>>();
                    logger?.LogWarning(
                        "Download bloqueado pelo throttle global: {LogsCategory} {Outcome} {ThrottleName} {Ip} {Slug}",
                        RateLimitLogging.EBookDownloadCategory, "BlockedThrottle", Name, ip, slug);
                }
            }
        }
    }
}
