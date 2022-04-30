using ShareBook.Domain;

namespace ShareBook.Repository
{
    public class MeetupRepository : RepositoryGeneric<Meetup>, IMeetupRepository
    {
        public MeetupRepository(ApplicationDbContext context) : base(context)
        {

        }
    }
}