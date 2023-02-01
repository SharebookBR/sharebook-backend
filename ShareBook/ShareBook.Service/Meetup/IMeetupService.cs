using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Service.Generic;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public interface IMeetupService : IBaseService<Meetup>
    {
        public Task<IList<string>> FetchMeetups();
        IList<Meetup> Search(string criteria);
    }
}
