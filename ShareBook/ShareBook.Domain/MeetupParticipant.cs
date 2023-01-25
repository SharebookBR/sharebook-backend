using ShareBook.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareBook.Domain
{
    public class MeetupParticipant : BaseEntity
    {
        public Guid MeetupId { get; set; }
        public virtual Meetup Meetup { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }
}
