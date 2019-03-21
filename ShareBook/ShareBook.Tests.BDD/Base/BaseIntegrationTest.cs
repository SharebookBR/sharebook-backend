using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using ShareBook.Api;
using System.IO;
using System.Net.Http;

namespace ShareBook.Tests.BDD.Base
{
    public class BaseIntegrationTest
    {
        public TestServer Server { get; set; }
        public HttpClient Client { get; set; }

        public BaseIntegrationTest()
        {

            var builder = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json");
            var configuration = builder.Build();

            var webHostBuilder =
               new WebHostBuilder()
               .UseConfiguration(configuration)
               .UseEnvironment("Development") 
               .UseStartup<Startup>();

            Server = new TestServer(webHostBuilder);
            Client = Server.CreateClient();
        }
    }
}
