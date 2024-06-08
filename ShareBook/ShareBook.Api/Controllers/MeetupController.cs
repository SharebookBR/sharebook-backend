using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public PagedList<Meetup> Get(int? page, int? pageSize, bool upcoming = false)
        {
            return _meetupService.Get(upcoming ? x => x.Active && x.StartDate > DateTime.Now : x => x.Active && x.StartDate <= DateTime.Now, x => x.StartDate, page ?? 1, pageSize ?? 10);
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
        public async Task<IList<Meetup>> SearchAsync([FromQuery]string criteria)
        {
            return await _meetupService.SearchAsync(criteria);
        }
    }
}
