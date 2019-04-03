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
        private INotification _notificationcs;

        public NotificationController(INotification notificationcs, IUserService userService)
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


        [HttpPost("notificar/{UserId}")]
        public IActionResult Notificar([FromRoute]Guid UserId, [FromBody] NotificationOnesignalVM request)
        {
            request.PlayerId = string.IsNullOrEmpty(request.PlayerId) ? _userService.Find(UserId).PlayerId : request.PlayerId;

            //var notification = Mapper.Map<NotificationOnesignalVM, NotificationOnesignal>(request);
            var notification = Mapper.Map<NotificationOnesignal>(request);

            string result = _notificationcs.SendNotificationByUserId(notification);

            return Ok(result);
        }

    }
}
