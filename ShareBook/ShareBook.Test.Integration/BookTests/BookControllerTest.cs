using ShareBook.Test.Integration.Base;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ShareBook.Test.Integration.BookTests
{
    public class BookControllerTest : IntegrationTestBase
    {

        public BookControllerTest()
        {
            //Falta startar o banco com dados...
        }

        //[Fact]
        //public async Task ListTop15NewBook()
        //{
        //    var response = await Client.GetAsync("api/Book/Top15NewBooks");
        //    response.EnsureSuccessStatusCode();
        //    var responseString = await response.Content.ReadAsStringAsync();
        //    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        //}
    }
}
