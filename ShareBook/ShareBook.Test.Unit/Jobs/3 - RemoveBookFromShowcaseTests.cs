using Microsoft.Extensions.Logging;
using Moq;
using Sharebook.Jobs;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Domain.Enums;
using ShareBook.Repository;
using ShareBook.Service;
using ShareBook.Test.Unit.Mocks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ShareBook.Test.Unit.Jobs
{
    public class RemoveBookFromShowcaseTests
    {
        private readonly Mock<IJobHistoryRepository> _mockJobHistoryRepository = new();
        private readonly Mock<ILoggerFactory> _mockLoggerFactory = new();
        private readonly Mock<IBookService> _mockBookService = new();
        private readonly Mock<IEmailService> _mockEmailService = new();
        private readonly Mock<IEmailTemplate> _mockEmailTemplate = new();
        private const string HtmlMock = "<html>Example</html>";

        public RemoveBookFromShowcaseTests()
        {
            _mockEmailTemplate.Setup(s => s.GenerateHtmlFromTemplateAsync(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(HtmlMock);
            _mockBookService.Setup(s => s.UpdateAsync(It.IsAny<Book>())).ReturnsAsync((Book book) => new Result<Book>(book));
        }

        // Regressão do incidente de 20/08/2026: livro sem facilitador derrubava o job,
        // que então retentava a cada 5 minutos sem nunca concluir.
        [Fact]
        public async Task RenewChooseDateWhenBookHasNoFacilitatorAndNoInterested()
        {
            var donor = new User { Id = Guid.NewGuid(), Name = "DonorWithoutFacilitator", Email = "donor@example.com" };
            var book = BookMock.GetLordTheRings(donor);
            book.BookUsers = new List<BookUser>();
            book.Status = BookStatus.Available;
            _mockBookService.Setup(s => s.GetBooksChooseDateIsTodayOrLateAsync()).ReturnsAsync(new List<Book> { book });

            RemoveBookFromShowcase job = new RemoveBookFromShowcase(_mockBookService.Object, _mockJobHistoryRepository.Object, _mockEmailService.Object, _mockEmailTemplate.Object, _mockLoggerFactory.Object);

            JobHistory result = await job.WorkAsync();

            Assert.True(result.IsSuccess);
            Assert.Contains("vai ficar +10 dias na vitrine", result.Details);
            Assert.Equal(DateTime.Today.AddDays(10), book.ChooseDate);

            _mockEmailTemplate.Verify(c => c.GenerateHtmlFromTemplateAsync(It.Is<string>(v => v.Equals("ChooseDateRenewTemplate")), It.IsAny<object>()), Times.Once);
            _mockEmailService.Verify(c => c.SendAsync(donor.Email, donor.Name, HtmlMock, It.IsAny<string>(), false, true), Times.Once);
            _mockBookService.Verify(c => c.UpdateAsync(book), Times.Once);
        }
    }
}
