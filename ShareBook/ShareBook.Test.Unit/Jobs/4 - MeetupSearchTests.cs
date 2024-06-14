using Moq;
using Sharebook.Jobs;
using ShareBook.Service;
using System.Threading.Tasks;
using Xunit;
using ShareBook.Repository;
using Microsoft.Extensions.Configuration;
using ShareBook.Domain.Enums;
using System.Collections.Generic;
using ShareBook.Domain;

namespace ShareBook.Test.Unit.Jobs
{
    public class MeetupSearchTests
    {
        private readonly Mock<IJobHistoryRepository> _mockJobHistoryRepository = new();
        private readonly Mock<IMeetupService> _mockMeetupService = new();
        private readonly Mock<IConfiguration> _mockConfiguration = new();

        [Fact]
        public async Task MeetupSettingsDisabled_ShouldReturn_MeetupDisabled()
        {
            _mockConfiguration.SetupGet(s => s[It.IsAny<string>()]).Returns("false");
            MeetupSearch job = new MeetupSearch(_mockJobHistoryRepository.Object, _mockMeetupService.Object, _mockConfiguration.Object);

            JobResult result = await job.ExecuteAsync();
            Assert.Equal(JobResult.MeetupDisabled, result);
            _mockMeetupService.VerifyNoOtherCalls();
            _mockJobHistoryRepository.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task MeetupSettingsEnabled_ShouldReturn_MeetupsCorrectly()
        {
            List<string> mockedMeetups = new List<string> { "Meetup Mock 1", "Meetup Mock 2" };
            _mockConfiguration.SetupGet(s => s[It.IsAny<string>()]).Returns("true");
            _mockMeetupService.Setup(s => s.FetchMeetupsAsync()).ReturnsAsync(() => mockedMeetups);
            MeetupSearch job = new MeetupSearch(_mockJobHistoryRepository.Object, _mockMeetupService.Object, _mockConfiguration.Object);

            JobHistory result = await job.WorkAsync();
            Assert.Equal("MeetupSearch", result.JobName);
            Assert.True(result.IsSuccess);
            Assert.Equal(string.Join("\n", mockedMeetups), result.Details);
            _mockMeetupService.Verify(c => c.FetchMeetupsAsync(), Times.Once);
            _mockMeetupService.VerifyNoOtherCalls();
            _mockJobHistoryRepository.VerifyNoOtherCalls();
        }
    }
}
