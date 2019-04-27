using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;

namespace ShareBook.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var appkey = AppDomain.CurrentDomain.BaseDirectory;
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path.Combine(appkey, "Files", "sharebook-app-key.json"));
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
