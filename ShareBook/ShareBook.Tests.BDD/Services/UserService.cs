using Newtonsoft.Json;
using ShareBook.Api.ViewModels;
using ShareBook.Tests.BDD.Base;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ShareBook.Tests.BDD.Services
{
    public class UserService : BaseIntegrationTest
    {
        public async Task<HttpResponseMessage> RegisterAsync(RegisterUserVM viewModel)
        {
            try
            {
                
                string entity = JsonConvert.SerializeObject(viewModel);

                var response = await Client.PostAsync("/api/account/register", new StringContent(entity, Encoding.UTF8, "application/json"));
                return response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                throw e;
            }
           
          
        }
    }
}
