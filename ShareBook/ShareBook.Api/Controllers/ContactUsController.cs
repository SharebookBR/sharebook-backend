using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Api.Filters;
using ShareBook.Api.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShareBook.Api.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowAllHeaders")]
    [GetClaimsFilter]
    public class ContactUsController : Controller
    {

        public ContactUsController()
        {

        }
        [HttpPost("Send")]
        public IActionResult Send([FromBody]ContactUsVM contactUsVM)
        {

            return NotFound();
        }
    }
}
