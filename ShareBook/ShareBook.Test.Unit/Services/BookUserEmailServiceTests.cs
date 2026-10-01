using Microsoft.Extensions.Caching.Memory;
using Moq;
using ShareBook.Service;
using ShareBook.Service.Notification;
using ShareBook.Test.Unit.Mocks;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ShareBook.Test.Unit.Services;

public class BookUserEmailServiceTests
{
    private readonly Mock<IUserService> _mockUserService = new();
    private readonly Mock<IEmailService> _mockEmailService = new();
    private readonly Mock<IEmailTemplate> _mockEmailTemplate = new();
    private readonly Mock<IPushNotificationService> _mockNotificationService = new();

    [Fact]
    public async Task SendEmailMaxRequestsUsesSuccessCopy()
    {
        var donor = UserMock.GetDonor();
        var book = BookMock.GetLordTheRings(donor);
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var service = new BookUserEmailService(
            _mockUserService.Object,
            _mockEmailService.Object,
            _mockEmailTemplate.Object,
            _mockNotificationService.Object,
            memoryCache,
            TimeProvider.System);

        await service.SendEmailMaxRequestsAsync(book);

        _mockEmailService.Verify(s => s.SendAsync(
            donor.Email,
            donor.Name,
            It.Is<string>(body =>
                body.Contains($"Que notícia boa: o livro <b>{book.Title}</b> fez sucesso no Sharebook.") &&
                body.Contains("Poucos livros chegam a esse ponto") &&
                body.Contains("A data da decisão foi antecipada para <b>amanhã</b>. Você receberá um e-mail com todas as informações.") &&
                body.Contains("Obrigado por ajudar esse livro a encontrar uma nova casa.") &&
                !body.Contains("atingiu o limite")),
            "Sua doação foi um sucesso",
            true,
            true),
            Times.Once);
        _mockEmailService.VerifyNoOtherCalls();
    }
}
