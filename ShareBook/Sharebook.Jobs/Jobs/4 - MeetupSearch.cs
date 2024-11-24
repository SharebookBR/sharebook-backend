using ShareBook.Repository;
using ShareBook.Service;
using ShareBook.Domain.Enums;
using System;
using ShareBook.Domain;
using ShareBook.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Sharebook.Jobs;

public class MeetupSearch : GenericJob, IJob
{
    private readonly IMeetupService _meetupService;
    private readonly IConfiguration _configuration;
    public MeetupSearch(IJobHistoryRepository jobHistoryRepo, IMeetupService meetupService, IConfiguration configuration) : base(jobHistoryRepo)
    {
        _meetupService = meetupService;

        JobName = "MeetupSearch";
        Description = "Atualiza a lista de Meetups do Sharebook com base no youtube.";
        Interval = Interval.Dayly;
        Active = true;
        BestTimeToExecute = new TimeSpan(1, 0, 0);
        _configuration = configuration;
    }

    public override async Task<JobHistory> WorkAsync()
    {
        var meetupEnabled = bool.Parse(_configuration["MeetupSettings:IsActive"]);
        if(!meetupEnabled) throw new MeetupDisabledException("Serviço Meetup está desabilitado no appsettings.");
        
        var jobResult = await _meetupService.FetchMeetupsAsync();

        return new JobHistory()
        {
            JobName = JobName,
            IsSuccess = true,
            Details = string.Join("\n", jobResult)
        };
    }
}
