using Moq;
using Sharebook.Jobs;
using ShareBook.Service;
using System.Threading.Tasks;
using Xunit;
using ShareBook.Repository;
using Microsoft.Extensions.Configuration;
using ShareBook.Domain.Enums;

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
        }
    }
}
