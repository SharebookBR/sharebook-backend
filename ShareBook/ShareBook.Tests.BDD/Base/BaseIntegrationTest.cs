using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using ShareBook.Api;
using System.Net.Http;

namespace ShareBook.Tests.BDD.Base
{
    public class BaseIntegrationTest
    {
        public TestServer Server { get; set; }
        public HttpClient Client { get; set; }

        public BaseIntegrationTest()
        {
            Server = new TestServer(new WebHostBuilder()
                    .UseStartup<Startup>());
            Client = Server.CreateClient();
        }
    }
}
