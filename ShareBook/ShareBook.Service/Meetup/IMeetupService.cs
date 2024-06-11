using ShareBook.Domain;
using ShareBook.Service.Generic;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public interface IMeetupService : IBaseService<Meetup>
    {
        public Task<IList<string>> FetchMeetupsAsync();
        Task<IList<Meetup>> SearchAsync(string criteria);
    }
}
