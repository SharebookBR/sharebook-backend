using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Rollbar.PlugIns.Serilog;
using Serilog;
using Serilog.Events;
using System;

namespace ShareBook.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
                .UseSerilog((ctx, lc) =>
                {
                    lc.ReadFrom.Configuration(ctx.Configuration)
                      .Enrich.FromLogContext()
                      .WriteTo.Console();

                    var rollbarToken = ctx.Configuration["Rollbar__Token"]
                        ?? Environment.GetEnvironmentVariable("Rollbar__Token");
                    var rollbarEnv = ctx.Configuration["Rollbar__Environment"]
                        ?? Environment.GetEnvironmentVariable("Rollbar__Environment")
                        ?? "Production";

                    if (!string.IsNullOrEmpty(rollbarToken))
                    {
                        lc.WriteTo.RollbarSink(
                            rollbarAccessToken: rollbarToken,
                            rollbarEnvironment: rollbarEnv,
                            restrictedToMinimumLevel: LogEventLevel.Error);
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .Build()
                .Run();
        }
    }
}
