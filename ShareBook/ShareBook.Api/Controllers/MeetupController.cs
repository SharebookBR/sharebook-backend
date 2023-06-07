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
        public PagedList<Meetup> Get(int? page, int? pageSize, bool? upcoming)
        {
            if(upcoming == null) return _meetupService.Get(x => x.StartDate, page ?? 1, pageSize ?? 10);

            return _meetupService.Get((bool)upcoming ? x => x.StartDate > DateTime.Now : x => x.StartDate <= DateTime.Now, x => x.StartDate, page ?? 1, pageSize ?? 10);

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

        [HttpGet("Search")]
        public IList<Meetup> Search([FromQuery]string criteria)
        {
            return _meetupService.Search(criteria);
        }
    }
}
