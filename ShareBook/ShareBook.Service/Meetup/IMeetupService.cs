using ShareBook.Domain;
using ShareBook.Service.Generic;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public interface IMeetupService : IBaseService<Meetup>
    {
        public Task<string> FetchMeetups();
    }
}
