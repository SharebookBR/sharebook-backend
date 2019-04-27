using AutoMapper;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Api.Filters;
using ShareBook.Api.ViewModels;
using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Helper;
using ShareBook.Service;
using ShareBook.Service.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShareBook.Api.Controllers
{
    [Route("api/[controller]")]
    [GetClaimsFilter]
    [EnableCors("AllowAllHeaders")]
    public class NotificationController : Controller
    {
        private readonly IUserService _userService;
        private INotificationService _notificationcs;

        public NotificationController(INotificationService notificationcs, IUserService userService)
        {
            _notificationcs = notificationcs;
            _userService = userService;
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
                Message = "Ping!"
            };

            return Ok(result);
        }


        [HttpPost("notification/")]
        public IActionResult NotificationByEmail([FromBody] NotificationOnesignalVM request)
        {
            var notification = Mapper.Map<NotificationOnesignal>(request);

            string result = _notificationcs.SendNotificationByEmail(notification.Value,notification.Title,notification.Content);

            return Ok(result);
        }

    }
}
