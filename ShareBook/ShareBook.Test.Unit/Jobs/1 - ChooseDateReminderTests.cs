using Moq;
using Sharebook.Jobs;
using ShareBook.Service;
using System.Threading.Tasks;
using Xunit;
using ShareBook.Repository;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using ShareBook.Domain;
using ShareBook.Domain.DTOs;
using ShareBook.Test.Unit.Mocks;
using ShareBook.Domain.Common;
using System.Linq;
using System;
using Xunit.Extensions.Ordering;

namespace ShareBook.Test.Unit.Jobs
{
    public class ChooseDateReminderTests
    {
        private readonly Mock<IJobHistoryRepository> _mockJobHistoryRepository = new();
        private readonly Mock<IBookService> _mockBookService = new();
        private readonly Mock<IEmailService> _mockEmailService = new();
        private readonly Mock<IEmailTemplate> _mockEmailTemplate = new();
        private const string HtmlMock = "<html>Example</html>";
        private static User _user = new User { Id = Guid.NewGuid(), Name = "TestUser", Email = "testuser@example.com" };
        private static Book _book = BookMock.GetLordTheRings(_user, _user);
        private static List<Book> _allBooks = new List<Book> { _book };

        public ChooseDateReminderTests()
        {
            _mockBookService.Setup(s => s.GetBooksChooseDateIsTodayAsync()).ReturnsAsync(_allBooks);
            _mockEmailTemplate.Setup(s => s.GenerateHtmlFromTemplateAsync(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(HtmlMock);
        }

        [Fact]
        public async Task SendReminderToTheUser()
        {
            ChooseDateReminder job = new ChooseDateReminder(_mockJobHistoryRepository.Object, _mockBookService.Object, _mockEmailService.Object, _mockEmailTemplate.Object);

            JobHistory result = await job.WorkAsync();

            Assert.True(result.IsSuccess);
            Assert.Equal("Lembrete amigável enviado para 'TestUser' referente ao livro 'Lord of the Rings'.", result.Details);
            Assert.Equal(ChooseDateReminder.JobName, result.JobName);

            _mockBookService.Verify(c => c.GetBooksChooseDateIsTodayAsync(), Times.Once);
            _mockEmailTemplate.Verify(c => c.GenerateHtmlFromTemplateAsync(It.Is<string>(v => v.Equals("ChooseDateReminderTemplate")), It.IsAny<object>()), Times.Once);
            _mockEmailService.Verify(c => c.SendAsync(It.Is<string>(v => v.Equals(_user.Email)), It.Is<string>(v => v.Equals(_user.Name)), It.Is<string>(v => v.Equals(HtmlMock)), It.Is<string>(v => v.Equals(ChooseDateReminder.EmailSubject)), It.Is<bool>((v) => !v), It.Is<bool>((v) => v)), Times.Once);
            
            _mockBookService.VerifyNoOtherCalls();
            _mockEmailService.VerifyNoOtherCalls();
            _mockEmailTemplate.VerifyNoOtherCalls();
        }
    }
}
