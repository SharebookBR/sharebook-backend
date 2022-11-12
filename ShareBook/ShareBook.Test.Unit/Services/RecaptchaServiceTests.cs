using ShareBook.Domain.Common;
using ShareBook.Service.Recaptcha;
using Xunit;

namespace ShareBook.Test.Unit.Services
{
    public class RecaptchaServiceTests
    {
        private readonly IRecaptchaService _recaptchaService = new RecaptchaService();
        private const string validRecaptcha = "asdaasdjasodaj7i364yubki23y728374234b2jk34h2347i26348724yh2bjhk34g2j34t273842384iuh2h4j234g2j34t27834bjh";
        private const string invalidRecaptcha = "123456";
        public RecaptchaServiceTests() { }


        [Fact]
        public void ValidRecaptcha()
        {
            Result result = _recaptchaService.SimpleValidationRecaptcha(validRecaptcha);
            Assert.True(result.Success);
        }

        [Fact]
        public void InvalidRecaptcha()
        {
            Result result = _recaptchaService.SimpleValidationRecaptcha(invalidRecaptcha);
            Assert.False(result.Success);
        }
    }
}
