using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
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
            var webHostBuilder =
               new WebHostBuilder()
               .UseSetting("ConnectionStrings:DefaultConnection", "Data Source=SQL7003.site4now.net;Initial Catalog=DB_A3BA78_sharebookdev;Integrated Security=False;User ID=DB_A3BA78_sharebookdev_admin;Password=teste123@;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False")
                     .UseEnvironment("Development") // You can set the environment you want (development, staging, production)
                     .UseStartup<Startup>(); // Startup class of your web app project

            Server = new TestServer(webHostBuilder);
            Client = Server.CreateClient();
        }
    }
}
