using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Service;
using System;
using System.Collections.Generic;

namespace ShareBook.Api.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowAllHeaders")]
    public class MeetupController : ControllerBase
    {
        private readonly IMeetupService _meetupService;
        public MeetupController(IMeetupService meetupService)
        {
            _meetupService = meetupService;
        }
        [HttpGet]
        public PagedList<Meetup> Get()
        {
            return _meetupService.Get(x => x.StartDate, 1, 50);
        }

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            if (!Guid.TryParse(id, out var meetupId))
            {
                BadRequest();
            }
            var meetup = _meetupService.Find(x => x.Id == meetupId);
            return meetup != null ? Ok(meetup) : NotFound();
        }
    }
}
