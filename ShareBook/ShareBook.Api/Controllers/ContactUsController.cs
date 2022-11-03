using AutoMapper;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Api.ViewModels;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Service;

namespace ShareBook.Api.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowAllHeaders")]
    public class ContactUsController : ControllerBase
    {
        private readonly IContactUsService _contactUsService;
        private readonly IMapper _mapper;

        public ContactUsController(IContactUsService contactUsService,
                                   IMapper mapper)
        {
            _contactUsService = contactUsService;
            _mapper = mapper;
        }

        [HttpPost("SendMessage")]
        public Result<ContactUs> SendMessage([FromBody]ContactUsVM contactUsVM)
        {
            var contactUS = _mapper.Map<ContactUs>(contactUsVM);

            return _contactUsService.SendContactUs(contactUS, contactUsVM?.RecaptchaReactive);
        }
    }
}