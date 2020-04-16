using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Api.Filters;
using ShareBook.Helper;
using System;

namespace ShareBook.Api.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowAllHeaders")]
    public class OperationsController : ControllerBase
    {
        [HttpGet]
        [Authorize("Bearer")]
        [AuthorizationFilter]
        [Route("ForceException")]
        public IActionResult ForceException()
        {
            var teste = 1 / Convert.ToInt32("Teste");
            return BadRequest();
        }

        [HttpGet("Ping")]
        public IActionResult Ping()
        {
            var result = new
            {
                ServerNow = DateTime.Now,
                SaoPauloNow = DateTimeHelper.ConvertDateTimeSaoPaulo(DateTime.Now),
                ServerToday = DateTime.Today,
                SaoPauloToday = DateTimeHelper.GetTodaySaoPaulo(),
                Message = "Pong!"
            };
            return Ok(result);
        }
    }
}
