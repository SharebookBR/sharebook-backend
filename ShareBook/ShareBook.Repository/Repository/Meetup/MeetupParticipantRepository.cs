using Microsoft.EntityFrameworkCore;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Repository.Repository;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ShareBook.Repository
{
    public class MeetupParticipantRepository : RepositoryGeneric<MeetupParticipant>, IMeetupParticipantRepository
    {
        public MeetupParticipantRepository(ApplicationDbContext context) : base(context)
        {

        }

        
    }
}