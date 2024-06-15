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
using System;
using Xunit.Extensions.Ordering;
using ShareBook.Domain.Enums;

namespace ShareBook.Test.Unit.Jobs
{
    public class LateDonationNotificationTests
    {
        private readonly Mock<IJobHistoryRepository> _mockJobHistoryRepository = new();
        private readonly Mock<IBookService> _mockBookService = new();
        private readonly Mock<IEmailService> _mockEmailService = new();
        private readonly Mock<IEmailTemplate> _mockEmailTemplate = new();
        private readonly Mock<IConfiguration> _mockConfiguration = new();
        private static int _maxLateDonationDays = 90;
        private const string HtmlMock = "<html>Example</html>";
        private static User _softUser = new User { Id = Guid.NewGuid(), Name = "SoftUser", Email = "softuser@example.com" };
        private static User _hardUser = new User { Id = Guid.NewGuid(), Name = "HardUser", Email = "harduser@example.com" };
        private static Book _softBook = BookMock.GetLordTheRings(_softUser, _softUser); 
        private static Book _hardBook = BookMock.GetLordTheRings(_hardUser, _hardUser);
        //private static List<Book> _allBooks = new List<Book> { _book1 };

        public LateDonationNotificationTests()
        {
            _hardBook.ChooseDate = DateTime.Now.AddDays((_maxLateDonationDays + 2) * -1);
            _hardBook.Status = BookStatus.AwaitingDonorDecision;
            _hardUser.BooksDonated = new List<Book> { _hardBook };
            _mockConfiguration.SetupGet(s => s[It.IsAny<string>()]).Returns(_maxLateDonationDays.ToString());
            _mockBookService.Setup(s => s.GetStatsAsync()).ReturnsAsync(new BookStatsDTO { TotalLate = 1, TotalOk = 10 });
            _mockEmailTemplate.Setup(s => s.GenerateHtmlFromTemplateAsync(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(HtmlMock);
        }

        [Fact, Order(1)]
        public async Task SendSoftEmailToTheUserAndToAdmins_1BookLate()
        {
            _mockBookService.Setup(s => s.GetBooksChooseDateIsLateAsync()).ReturnsAsync(new List<Book> { _softBook });
            LateDonationNotification job = new LateDonationNotification(_mockJobHistoryRepository.Object, _mockBookService.Object, _mockEmailService.Object, _mockEmailTemplate.Object, _mockConfiguration.Object);

            JobHistory result = await job.WorkAsync();

            Assert.True(result.IsSuccess);
            Assert.Equal($"Encontradas 1 doações em atraso de 1 doadores distintos.E-mail enviado para o usuário: {_softUser.Name}", result.Details);
            Assert.Equal(LateDonationNotification.JobName, result.JobName);

            _mockConfiguration.Verify(c => c[LateDonationNotification.ConfigMaxLateDonationDaysKey], Times.Once);
            _mockEmailTemplate.Verify(c => c.GenerateHtmlFromTemplateAsync(It.Is<string>(v => v.Equals(LateDonationNotification.EmailTemplateName)), It.IsAny<object>()), Times.Once);

            _mockBookService.Verify(c => c.GetBooksChooseDateIsLateAsync(), Times.Once);
            _mockBookService.Verify(c => c.GetStatsAsync(), Times.Once);

            _mockEmailService.Verify(c => c.SendToAdminsAsync(HtmlMock, LateDonationNotification.EmailAdminsSubject), Times.Once);
            _mockEmailService.Verify(c => c.SendAsync(_softUser.Email, _softUser.Name, It.IsAny<string>(), LateDonationNotification.EmailDonatorSoftSubject, false, true), Times.Once);


            _mockConfiguration.VerifyNoOtherCalls();
            _mockEmailTemplate.VerifyNoOtherCalls();
            _mockBookService.VerifyNoOtherCalls();
            _mockEmailService.VerifyNoOtherCalls();
        }

        [Fact, Order(2)]
        public async Task SendHardEmailToTheUserAndToAdmins_1BookLate()
        {
            _mockBookService.Setup(s => s.GetBooksChooseDateIsLateAsync()).ReturnsAsync(new List<Book> { _hardBook });
            LateDonationNotification job = new LateDonationNotification(_mockJobHistoryRepository.Object, _mockBookService.Object, _mockEmailService.Object, _mockEmailTemplate.Object, _mockConfiguration.Object);

            JobHistory result = await job.WorkAsync();

            Assert.True(result.IsSuccess);
            Assert.Equal($"Encontradas 1 doações em atraso de 1 doadores distintos.E-mail enviado para o usuário: {_hardUser.Name}", result.Details);
            Assert.Equal(LateDonationNotification.JobName, result.JobName);

            _mockConfiguration.Verify(c => c[LateDonationNotification.ConfigMaxLateDonationDaysKey], Times.Once);
            _mockEmailTemplate.Verify(c => c.GenerateHtmlFromTemplateAsync(It.Is<string>(v => v.Equals(LateDonationNotification.EmailTemplateName)), It.IsAny<object>()), Times.Once);

            _mockBookService.Verify(c => c.GetBooksChooseDateIsLateAsync(), Times.Once);
            _mockBookService.Verify(c => c.GetStatsAsync(), Times.Once);

            _mockEmailService.Verify(c => c.SendToAdminsAsync(HtmlMock, LateDonationNotification.EmailAdminsSubject), Times.Once);
            _mockEmailService.Verify(c => c.SendAsync(_hardUser.Email, _hardUser.Name, It.IsAny<string>(), LateDonationNotification.EmailDonatorHardSubject, true, true), Times.Once);


            _mockConfiguration.VerifyNoOtherCalls();
            _mockEmailTemplate.VerifyNoOtherCalls();
            _mockBookService.VerifyNoOtherCalls();
            _mockEmailService.VerifyNoOtherCalls();
        }

        [Fact, Order(3)]
        public async Task NotSendAnyEmail_0BooksLate()
        {
            _mockBookService.Setup(s => s.GetBooksChooseDateIsLateAsync()).ReturnsAsync(new List<Book>());
            LateDonationNotification job = new LateDonationNotification(_mockJobHistoryRepository.Object, _mockBookService.Object, _mockEmailService.Object, _mockEmailTemplate.Object, _mockConfiguration.Object);

            JobHistory result = await job.WorkAsync();

            Assert.True(result.IsSuccess);
            Assert.Equal("Encontradas 0 doações em atraso de 0 doadores distintos.", result.Details);
            Assert.Equal(LateDonationNotification.JobName, result.JobName);

            _mockConfiguration.Verify(c => c[LateDonationNotification.ConfigMaxLateDonationDaysKey], Times.Once);
            _mockBookService.Verify(c => c.GetBooksChooseDateIsLateAsync(), Times.Once);
            _mockBookService.Verify(c => c.GetStatsAsync(), Times.Once);

            _mockConfiguration.VerifyNoOtherCalls();
            _mockEmailTemplate.VerifyNoOtherCalls();
            _mockBookService.VerifyNoOtherCalls();
            _mockEmailService.VerifyNoOtherCalls();
        }
    }
}
