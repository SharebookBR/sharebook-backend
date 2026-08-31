using Moq;
using Sharebook.Jobs;
using ShareBook.Service;
using System.Threading.Tasks;
using Xunit;
using ShareBook.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using ShareBook.Domain;
using ShareBook.Domain.DTOs;
using ShareBook.Test.Unit.Mocks;
using System;
using ShareBook.Domain.Enums;

namespace ShareBook.Test.Unit.Jobs
{
    public class LateDonationNotificationTests
    {
        private readonly Mock<IJobHistoryRepository> _mockJobHistoryRepository = new();
        private readonly Mock<ILoggerFactory> _mockLoggerFactory = new();
        private readonly Mock<IBookService> _mockBookService = new();
        private readonly Mock<IEmailService> _mockEmailService = new();
        private readonly Mock<IEmailTemplate> _mockEmailTemplate = new();
        private readonly Mock<IConfiguration> _mockConfiguration = new();
        private const int _maxLateDonationDays = 90;
        private const string HtmlMock = "<html>Example</html>";
        private readonly User _softUser = new User { Id = Guid.NewGuid(), Name = "SoftUser", Email = "softuser@example.com" };
        private readonly User _hardUser = new User { Id = Guid.NewGuid(), Name = "HardUser", Email = "harduser@example.com" };
        private readonly Book _softBook;
        private readonly Book _hardBook;

        public LateDonationNotificationTests()
        {
            _softBook = BookMock.GetLordTheRings(_softUser, _softUser);
            _hardBook = BookMock.GetLordTheRings(_hardUser, _hardUser);
            _hardBook.ChooseDate = DateTime.UtcNow.AddDays((_maxLateDonationDays + 2) * -1);
            _hardBook.Status = BookStatus.AwaitingDonorDecision;
            _hardUser.BooksDonated = new List<Book> { _hardBook };
            _mockConfiguration.SetupGet(s => s[It.IsAny<string>()]).Returns(_maxLateDonationDays.ToString());
            _mockBookService.Setup(s => s.GetStatsAsync()).ReturnsAsync(new BookStatsDTO { TotalLate = 1, TotalOk = 10 });
            _mockEmailTemplate.Setup(s => s.GenerateHtmlFromTemplateAsync(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(HtmlMock);
        }

        [Fact]
        public async Task SendSoftEmailToTheUserAndToAdmins_1BookLate()
        {
            _mockBookService.Setup(s => s.GetBooksChooseDateIsLateAsync()).ReturnsAsync(new List<Book> { _softBook });
            LateDonationNotification job = new LateDonationNotification(_mockJobHistoryRepository.Object, _mockBookService.Object, _mockEmailService.Object, _mockEmailTemplate.Object, _mockLoggerFactory.Object, _mockConfiguration.Object);

            JobHistory result = await job.WorkAsync();

            Assert.True(result.IsSuccess);
            Assert.Equal($"Encontradas 1 doações em atraso de 1 doadores distintos.E-mail enviado para o usuário: {_softUser.Name}", result.Details);

            _mockConfiguration.Verify(c => c[LateDonationNotification.ConfigMaxLateDonationDaysKey], Times.Once);
            _mockEmailTemplate.Verify(c => c.GenerateHtmlFromTemplateAsync(It.Is<string>(v => v.Equals(LateDonationNotification.EmailTemplateName)), It.IsAny<object>()), Times.Once);

            _mockBookService.Verify(c => c.GetBooksChooseDateIsLateAsync(), Times.Once);
            _mockBookService.Verify(c => c.GetStatsAsync(), Times.Once);

            _mockEmailService.Verify(c => c.SendToAdminsAsync(HtmlMock, LateDonationNotification.EmailAdminsSubject), Times.Once);
            Assert.Equal("Só falta escolher quem vai receber", LateDonationNotification.EmailDonatorSoftSubject);
            _mockEmailService.Verify(c => c.SendAsync(
                _softUser.Email,
                _softUser.Name,
                It.Is<string>(html =>
                    html.Contains("Tem gente interessada em receber um livro") &&
                    html.Contains("Escolher ganhador(a)") &&
                    html.Contains("fale com a gente") &&
                    html.Contains("Equipe Sharebook") &&
                    html.Contains("Compartilhando conhecimento") &&
                    !html.Contains("Para sua conveniência") &&
                    !html.Contains("=)")),
                LateDonationNotification.EmailDonatorSoftSubject,
                false,
                true), Times.Once);


            _mockConfiguration.VerifyNoOtherCalls();
            _mockEmailTemplate.VerifyNoOtherCalls();
            _mockBookService.VerifyNoOtherCalls();
            _mockEmailService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task SendHardEmailToTheUserAndToAdmins_1BookLate()
        {
            _mockBookService.Setup(s => s.GetBooksChooseDateIsLateAsync()).ReturnsAsync(new List<Book> { _hardBook });
            LateDonationNotification job = new LateDonationNotification(_mockJobHistoryRepository.Object, _mockBookService.Object, _mockEmailService.Object, _mockEmailTemplate.Object, _mockLoggerFactory.Object, _mockConfiguration.Object);

            JobHistory result = await job.WorkAsync();

            Assert.True(result.IsSuccess);
            Assert.Equal($"Encontradas 1 doações em atraso de 1 doadores distintos.E-mail enviado para o usuário: {_hardUser.Name}", result.Details);

            _mockConfiguration.Verify(c => c[LateDonationNotification.ConfigMaxLateDonationDaysKey], Times.Once);
            _mockEmailTemplate.Verify(c => c.GenerateHtmlFromTemplateAsync(It.Is<string>(v => v.Equals(LateDonationNotification.EmailTemplateName)), It.IsAny<object>()), Times.Once);

            _mockBookService.Verify(c => c.GetBooksChooseDateIsLateAsync(), Times.Once);
            _mockBookService.Verify(c => c.GetStatsAsync(), Times.Once);

            _mockEmailService.Verify(c => c.SendToAdminsAsync(HtmlMock, LateDonationNotification.EmailAdminsSubject), Times.Once);
            Assert.Equal("Último aviso sobre sua doação", LateDonationNotification.EmailDonatorHardSubject);
            _mockEmailService.Verify(c => c.SendAsync(
                _hardUser.Email,
                _hardUser.Name,
                It.Is<string>(html =>
                    html.Contains($"mais de {_maxLateDonationDays} dias") &&
                    html.Contains("escolher o(a) ganhador(a) ou cancelar") &&
                    html.Contains("Resolver minha doação") &&
                    html.Contains("Este é o último aviso") &&
                    html.Contains("sua conta será bloqueada") &&
                    !html.Contains("Pessoas humildes") &&
                    !html.Contains(" vc ") &&
                    !html.Contains("Para sua conveniência")),
                LateDonationNotification.EmailDonatorHardSubject,
                true,
                true), Times.Once);


            _mockConfiguration.VerifyNoOtherCalls();
            _mockEmailTemplate.VerifyNoOtherCalls();
            _mockBookService.VerifyNoOtherCalls();
            _mockEmailService.VerifyNoOtherCalls();
        }

        // Regressão do incidente de 20/08/2026: facilitador é opcional no livro, e
        // um único livro sem facilitador derrubava o job inteiro.
        [Fact]
        public async Task SendEmailsWhenBookHasNoFacilitator()
        {
            var donor = new User { Id = Guid.NewGuid(), Name = "DonorWithoutFacilitator", Email = "donor@example.com" };
            var bookWithoutFacilitator = BookMock.GetLordTheRings(donor);
            _mockBookService.Setup(s => s.GetBooksChooseDateIsLateAsync()).ReturnsAsync(new List<Book> { bookWithoutFacilitator });

            object adminVm = null;
            _mockEmailTemplate
                .Setup(s => s.GenerateHtmlFromTemplateAsync(It.IsAny<string>(), It.IsAny<object>()))
                .Callback<string, object>((_, vm) => adminVm = vm)
                .ReturnsAsync(HtmlMock);

            LateDonationNotification job = new LateDonationNotification(_mockJobHistoryRepository.Object, _mockBookService.Object, _mockEmailService.Object, _mockEmailTemplate.Object, _mockLoggerFactory.Object, _mockConfiguration.Object);

            JobHistory result = await job.WorkAsync();

            Assert.True(result.IsSuccess);
            Assert.Equal($"Encontradas 1 doações em atraso de 1 doadores distintos.E-mail enviado para o usuário: {donor.Name}", result.Details);

            // A aposentadoria do facilitador começou pela experiência visível:
            // o relatório continua útil sem expor essa função operacional.
            var htmlTable = adminVm.GetType().GetProperty("htmlTable").GetValue(adminVm) as string;
            Assert.Contains(donor.Name, htmlTable);
            Assert.DoesNotContain("FACILITADOR", htmlTable);

            _mockEmailService.Verify(c => c.SendToAdminsAsync(HtmlMock, LateDonationNotification.EmailAdminsSubject), Times.Once);
        }

        [Fact]
        public async Task NotSendAnyEmail_0BooksLate()
        {
            _mockBookService.Setup(s => s.GetBooksChooseDateIsLateAsync()).ReturnsAsync(new List<Book>());
            LateDonationNotification job = new LateDonationNotification(_mockJobHistoryRepository.Object, _mockBookService.Object, _mockEmailService.Object, _mockEmailTemplate.Object, _mockLoggerFactory.Object, _mockConfiguration.Object);

            JobHistory result = await job.WorkAsync();

            Assert.True(result.IsSuccess);
            Assert.Equal("Encontradas 0 doações em atraso de 0 doadores distintos.", result.Details);

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
