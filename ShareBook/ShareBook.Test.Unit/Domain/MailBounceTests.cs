using ShareBook.Domain;
using Xunit;

namespace ShareBook.Test.Unit.Domain
{
    public class MailBounceTests
    {
        [Fact]
        public void Constructor_ShouldParseLegacyRemoteServerReturnedBounce()
        {
            var body = "Diagnostic-Code: smtp; Remote Server returned: '550 5.1.1 user unknown'";

            var bounce = new MailBounce("Undeliverable", body);

            Assert.True(bounce.IsBounce);
            Assert.False(bounce.IsSoft);
            Assert.Equal("550", bounce.ErrorCode);
        }

        [Fact]
        public void Constructor_ShouldParseStalwartDeliveryStatusBounce()
        {
            var body = @"Your message could not be delivered to the following recipients:

<sharebook-bounce-test-1788869999@gmail.com> (host 'gmail-smtp-in.l.google.com' rejected command 'RCPT TO:<sharebook-bounce-test-1788869999@gmail.com>' with code 550 (5.1.1) 'The email account that you tried to reach does not exist.')";

            var bounce = new MailBounce("Failed to deliver message", body);

            Assert.True(bounce.IsBounce);
            Assert.False(bounce.IsSoft);
            Assert.Equal("550", bounce.ErrorCode);
            Assert.Equal("sharebook-bounce-test-1788869999@gmail.com", bounce.Email);
        }

        [Fact]
        public void Constructor_ShouldMarkFourHundredCodesAsSoftBounce()
        {
            var body = "<temporary@example.com> rejected with code 451 (4.7.1) try again later";

            var bounce = new MailBounce("Failed to deliver message", body);

            Assert.True(bounce.IsBounce);
            Assert.True(bounce.IsSoft);
            Assert.Equal("451", bounce.ErrorCode);
        }
    }
}
