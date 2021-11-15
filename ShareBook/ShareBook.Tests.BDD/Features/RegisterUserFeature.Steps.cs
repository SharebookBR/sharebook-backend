﻿using LightBDD.XUnit2;
using ShareBook.Api.ViewModels;
using ShareBook.Domain.DTOs;
using ShareBook.Tests.BDD.Services;
using System.Net;
using System.Net.Http;
using Xunit;

namespace ShareBook.Tests.BDD.Features
{
	public partial class RegisterUserFeature: FeatureFixture
	{
        private RegisterUserDTO viewModel;
        private HttpResponseMessage response;

        private void Given_new_user_want_to_join_with_this_datas()
		{
            viewModel = new RegisterUserDTO()
            {
                Name = "Joaquim",
                Password = "Joa.2019!",
                Email = "joaquim@gmail.com",
                City = "São Paulo",
                PostalCode = "04473-150",
                Linkedin = "linkedin.com/joaquim",
                Phone = "11 93354-0025",
                Neighborhood = "Vila Olimpia",
                Street = "Rua teste",
                Country = "Brasil",
                State = "SP",
                Number = "100"
            };         
		}

		private void When_the_new_user_registers()
		{
            UserService service = new UserService();
            response = service.RegisterAsync(viewModel).Result;
        }

		private void Then_will_receive_a_success_message()
		{
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
	}
}