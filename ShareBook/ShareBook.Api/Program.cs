using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace ShareBook.Api
{
    public static class Program
    {
        public static void Main()
        {
            WebHost.CreateDefaultBuilder().UseStartup<Startup>().Build();
        }
    }
}