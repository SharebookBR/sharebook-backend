using AutoMapper;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Api.Filters;
using ShareBook.Api.ViewModels;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShareBook.Api.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowAllHeaders")]
    public class ContactUsController : Controller
    {
        IContactUsService _contactUsService;

        public ContactUsController(IContactUsService contactUsService)
        {
            _contactUsService = contactUsService;
        }

        [HttpPost("SendMessage")]
        public IActionResult SendMessage([FromQuery(Name = "Mensagem")] string mensagem,
                                         [FromBody]ContactUsVM contactUsVM)
        {
            
            if (!ModelState.IsValid)
                return BadRequest();

            var contactUS = Mapper.Map<ContactUs>(contactUsVM);

            _contactUsService.SendContactUs(contactUS);

            var result = new Result
            {
                SuccessMessage = "Mensagem enviada com sucesso!",
            };

            return Ok(result);
        }
    }
}
