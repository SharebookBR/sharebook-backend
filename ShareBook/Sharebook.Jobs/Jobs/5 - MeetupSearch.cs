using ShareBook.Repository;
using ShareBook.Service;
using ShareBook.Domain.Enums;
using System;
using ShareBook.Domain;

namespace Sharebook.Jobs
{
    public class MeetupSearch : GenericJob, IJob
    {
        private readonly IMeetupService _meetupService;
        public MeetupSearch(IJobHistoryRepository jobHistoryRepo, IMeetupService meetupService) : base(jobHistoryRepo)
        {
            _meetupService = meetupService;

            JobName = "MeetupSearch";
            Description = "Mantém uma lista atualizada de eventos do sharebook no sympla juntamente com os links para a live no youtube";
            Interval = Interval.Dayly;
            Active = true;
            BestTimeToExecute = new TimeSpan(0, 0, 0);
        }

        public override JobHistory Work()
        {
            var jobResult = _meetupService.FetchMeetups().Result;

            return new JobHistory()
            {
                JobName = JobName,
                IsSuccess = true,
                Details = jobResult
            };
        }
    }
}
