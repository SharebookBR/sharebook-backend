using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;
using Serilog.Sinks.MSSqlServer;
using Serilog;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Extensions.Hosting;

namespace ShareBook.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var sinkOptions = new MSSqlServerSinkOptions
            {
                TableName = configuration["Serilog:TableName"],
                AutoCreateSqlTable = true,
                SchemaName = configuration["Serilog:SchemaName"]
            };

            Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
           .WriteTo.MSSqlServer(
               connectionString: configuration["Serilog:ConnectionLogs"],
               sinkOptions: sinkOptions,
               appConfiguration: configuration)
               .CreateLogger();

            CreateHostBuilder(args).Build().Run();
            Log.CloseAndFlush();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            }).UseSerilog();
    }
}