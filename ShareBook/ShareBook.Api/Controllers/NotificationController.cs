using AutoMapper;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Api.Filters;
using ShareBook.Api.ViewModels;
using ShareBook.Domain;
using ShareBook.Helper;
using ShareBook.Service;
using ShareBook.Service.Notification;
using System;

namespace ShareBook.Api.Controllers
{
    [Route("api/[controller]")]
    [GetClaimsFilter]
    [EnableCors("AllowAllHeaders")]
    public class NotificationController : Controller
    {
        private readonly IUserService _userService;
        private readonly INotificationService _notificationcs;
        private readonly IMapper _mapper;

        public NotificationController(INotificationService notificationcs,
                                      IUserService userService,
                                      IMapper mapper)
        {
            _notificationcs = notificationcs;
            _userService = userService;
            _mapper = mapper;
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
            var notification = _mapper.Map<NotificationOnesignal>(request);

            string result = _notificationcs.SendNotificationByEmail(notification.Value, notification.Title, notification.Content);

            return Ok(result);
        }
    }
}