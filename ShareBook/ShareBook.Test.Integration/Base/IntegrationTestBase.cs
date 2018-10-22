using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using ShareBook.Api;
using System.Net.Http;

namespace ShareBook.Test.Integration.Base
{
    public class IntegrationTestBase
    {
        public TestServer Server { get; set; }
        public HttpClient Client { get; set; }

        public IntegrationTestBase()
        {
            Server = new TestServer(new WebHostBuilder()
                    .UseStartup<Startup>());
            Client = Server.CreateClient();
        }
    }
}
