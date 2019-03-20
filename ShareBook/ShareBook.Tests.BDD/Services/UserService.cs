using Newtonsoft.Json;
using ShareBook.Api.ViewModels;
using ShareBook.Tests.BDD.Base;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ShareBook.Tests.BDD.Services
{
    public class UserService : BaseIntegrationTest
    {
        public async Task<string> RegisterAsync()
        {
            try
            {
                RegisterUserVM viewModel = new RegisterUserVM()
                {
                    Name = "Walter"
                };
                string entity = JsonConvert.SerializeObject(viewModel);

                var response = await Client.PostAsync("/api/account/register", new StringContent(entity, Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {

                throw e;
            }
           
          
        }
    }
}
