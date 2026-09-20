using ShareBook.Domain;
using Xunit;

namespace ShareBook.Test.Unit.Domain;

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

    [Fact]
    public void Constructor_ShouldParseOutlookRemoteServerReturnedBounce()
    {
        var body = @"Delivery has failed to these recipients or groups:

full-mailbox@example.com
The recipient's mailbox is full and can't accept messages now.
Remote server returned '554 5.2.2 mailbox full; STOREDRV.Deliver.Exception:QuotaExceededException'";

        var bounce = new MailBounce("Undeliverable: Novos livros digitais esta semana", body);

        Assert.True(bounce.IsBounce);
        Assert.False(bounce.IsSoft);
        Assert.Equal("554", bounce.ErrorCode);
        Assert.Equal("full-mailbox@example.com", bounce.Email);
    }

    [Fact]
    public void Constructor_ShouldTreatNoMxAsHardBounce()
    {
        var body = @"Your message could not be delivered to the following recipients:

<missing-domain@example.invalid> (failed to lookup 'example.invalid': no MX record found.)";

        var bounce = new MailBounce("Failed to deliver message", body);

        Assert.True(bounce.IsBounce);
        Assert.False(bounce.IsSoft);
        Assert.Equal("550", bounce.ErrorCode);
        Assert.Equal("missing-domain@example.invalid", bounce.Email);
    }

    [Fact]
    public void Constructor_ShouldTreatConnectionTimeoutAsSoftBounce()
    {
        var body = @"There was a temporary problem delivering your message to the following recipients:

<temporary@example.com> (connection to 'example.com' failed: I/O error: Connection timed out)";

        var bounce = new MailBounce("Warning: Delay in message delivery", body);

        Assert.True(bounce.IsBounce);
        Assert.True(bounce.IsSoft);
        Assert.Equal("421", bounce.ErrorCode);
        Assert.Equal("temporary@example.com", bounce.Email);
    }
}
