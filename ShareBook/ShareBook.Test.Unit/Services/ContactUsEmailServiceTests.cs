using Moq;
using ShareBook.Domain;
using ShareBook.Service;
using Xunit;
using System.Threading.Tasks;

namespace ShareBook.Test.Unit.Services
{
    public class ContactUsEmailServiceTests
    {
        private readonly Mock<IEmailService> _mockEmailService = new();
        private readonly Mock<IEmailTemplate> _mockEmailTemplate = new();
        private const string HtmlMock = "<html>Example</html>";

        public ContactUsEmailServiceTests()
        {
            _mockEmailService.Setup(t => t.SendToAdmins(It.IsAny<string>(), It.IsAny<string>()));
            _mockEmailService.Setup(t => t.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()));
            _mockEmailService.Setup(t => t.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
            _mockEmailTemplate.Setup(t => t.GenerateHtmlFromTemplateAsync(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(HtmlMock);
        }

        [Fact]
        public async Task SendEmailContactToTheUserAndToAdministrators()
        {
            ContactUs contactUs = new ContactUs
            {
                Email = "joazinho.souza@example.com",
                Message = "Test message test, Test message test, Test message test",
                Name = "Joãozinho",
                Phone = "44 9 8877-6655"
            };
            ContactUsEmailService service = new ContactUsEmailService(_mockEmailService.Object, _mockEmailTemplate.Object);
            await service.SendEmailContactUsAsync(contactUs);
            
            _mockEmailService.Verify(s => s.SendToAdmins(HtmlMock, ContactUsEmailService.ContactUsTitle), Times.Once);
            _mockEmailService.Verify(s => s.Send(contactUs.Email, contactUs.Name, HtmlMock, ContactUsEmailService.ContactUsNotificationTitle, false, true), Times.Once);
            _mockEmailService.VerifyNoOtherCalls();
            _mockEmailTemplate.Verify(s => s.GenerateHtmlFromTemplateAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Exactly(2));
            _mockEmailTemplate.VerifyNoOtherCalls();
        }
    }
}
