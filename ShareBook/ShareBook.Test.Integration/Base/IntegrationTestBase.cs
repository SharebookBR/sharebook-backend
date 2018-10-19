using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using ShareBook.Api;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

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
