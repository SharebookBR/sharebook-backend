using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NpgsqlTypes;
using Rollbar.PlugIns.Serilog;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.PostgreSQL.ColumnWriters;
using ShareBook.Api.Logging;
using ShareBook.Api.RateLimiting;
using System;
using System.Collections.Generic;

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
                        lc.WriteTo.Logger(rollbarLogger => rollbarLogger
                            .Filter.ByExcluding(RollbarLogEventFilter.ShouldExclude)
                            .WriteTo.RollbarSink(
                                rollbarAccessToken: rollbarToken,
                                rollbarEnvironment: rollbarEnv,
                                restrictedToMinimumLevel: LogEventLevel.Error));
                    }

                    var dbProvider = ctx.Configuration["DatabaseProvider"]?.ToLower();
                    var postgresConnection = ctx.Configuration.GetConnectionString("PostgresConnection");

                    if (dbProvider == "postgres" && !string.IsNullOrEmpty(postgresConnection))
                    {
                        var logsColumnWriters = new Dictionary<string, ColumnWriterBase>
                        {
                            { "Timestamp", new TimestampColumnWriter(NpgsqlDbType.TimestampTz) },
                            { "Level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
                            { "Message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
                            { "Exception", new ExceptionColumnWriter(NpgsqlDbType.Text) },
                            { "Properties", new PropertiesColumnWriter(NpgsqlDbType.Jsonb) },
                        };

                        // Só eventos marcados explicitamente (via RateLimitLogging.CategoryProperty)
                        // caem na tabela "Logs" — não é espelho do request log geral.
                        lc.WriteTo.Logger(sub => sub
                            .Filter.ByIncludingOnly(e => e.Properties.ContainsKey(RateLimitLogging.CategoryProperty))
                            .WriteTo.PostgreSQL(
                                connectionString: postgresConnection,
                                tableName: "Logs",
                                columnOptions: logsColumnWriters,
                                needAutoCreateTable: false));
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
