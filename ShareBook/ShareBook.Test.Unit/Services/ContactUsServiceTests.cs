using Moq;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Domain.Validators;
using ShareBook.Service;
using ShareBook.Service.Recaptcha;
using System.Threading.Tasks;
using Xunit;

namespace ShareBook.Test.Unit.Services
{
    public class ContactUsServiceTests
    {
        readonly Mock<IContactUsService> contactUsServiceMock;
        readonly Mock<IRecaptchaService> recaptchaServiceMock;
        readonly IRecaptchaService recaptchaService;
        readonly Mock<IContactUsEmailService> contactUsEmailServiceMock;

        public ContactUsServiceTests()
        {
            contactUsServiceMock = new Mock<IContactUsService>();
            recaptchaServiceMock = new Mock<IRecaptchaService>();
            recaptchaService = new RecaptchaService();
            contactUsEmailServiceMock = new Mock<IContactUsEmailService>();

        }
        [Fact]
        public async Task Invalid()
        {
            ContactUsService service = new ContactUsService(contactUsEmailServiceMock.Object, new ContactUsValidator(), recaptchaService);
            Result<ContactUs> result = await service.SendContactUsAsync(new ContactUs
            {
                Email = "joazinho",
                Message = "Test message test, Test message test, Test message test",
                Name = "Joãozinho",
                Phone = "44 9 8877-6655"
            }, "kasdhauiskhduiasydyaushdausytdsuadgausydhasdg7qwqgdqdgyuqwgdusyadgh7asiyda7sdtgadgashd");
            Assert.False(result.Success);
        }

        [Fact]
        public async Task Valid()
        {
            ContactUsService service = new ContactUsService(contactUsEmailServiceMock.Object, new ContactUsValidator(), recaptchaService);
            Result<ContactUs> result = await service.SendContactUsAsync(new ContactUs
            {
                Email = "joazinho.souza@example.com",
                Message = "Test message test, Test message test, Test message test",
                Name = "Joãozinho",
                Phone = "44 9 8877-6655"
            }, "12asdx5482asd56asd878as67d8kasdhauiskhduiasyd12a345asdaduhu12yaushdausytdsuadgausydhasdg7qwqgdqdgyuqwgdusyadgh7asiyda7sdtgadgashd");
            Assert.True(result.Success);
        }

        [Fact]
        public async Task InvalidRecaptcha()
        {
            ContactUsService service = new ContactUsService(contactUsEmailServiceMock.Object, new ContactUsValidator(), recaptchaService);
            Result<ContactUs> result = await service.SendContactUsAsync(new ContactUs
            {
                Email = "joazinho.souza@example.com",
                Message = "Test message test, Test message test, Test message test",
                Name = "Joãozinho",
                Phone = "44 9 8877-6655"
            }, "");
            Assert.False(result.Success);
        }
    }
}
