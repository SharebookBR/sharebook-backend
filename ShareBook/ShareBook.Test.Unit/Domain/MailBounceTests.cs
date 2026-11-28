using ShareBook.Domain;
using ShareBook.Service;
using MimeKit;
using System.IO;
using System.Text;
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

    [Fact]
    public void Constructor_ShouldParseStandardDsnStatusAndPreferFinalRecipient()
    {
        var body = @"Reporting-MTA: dns; mail.example.org

Final-Recipient: rfc822; bounced@example.com
Action: failed
Status: 5.1.1";

        var bounce = new MailBounce("Delivery Status Notification (Failure)", body);

        Assert.True(bounce.IsBounce);
        Assert.False(bounce.IsSoft);
        Assert.Equal("550", bounce.ErrorCode);
        Assert.Equal("bounced@example.com", bounce.Email);
    }

    [Fact]
    public void Constructor_ShouldParseTemporaryStandardDsnStatus()
    {
        var body = @"Final-Recipient: rfc822; retry@example.com
Action: delayed
Status: 4.2.0";

        var bounce = new MailBounce("Warning: Delay in message delivery", body);

        Assert.True(bounce.IsBounce);
        Assert.True(bounce.IsSoft);
        Assert.Equal("450", bounce.ErrorCode);
        Assert.Equal("retry@example.com", bounce.Email);
    }

    [Fact]
    public void Constructor_ShouldParsePortugueseDeliveryFailure()
    {
        var body = @"Não foi possível entregar a sua mensagem para o destinatário abaixo.
Destinatário: inexistente@example.com
Erro SMTP: 550 5.1.1 endereço não existe.";

        var bounce = new MailBounce("Falha na entrega", body);

        Assert.True(bounce.IsBounce);
        Assert.False(bounce.IsSoft);
        Assert.Equal("550", bounce.ErrorCode);
        Assert.Equal("inexistente@example.com", bounce.Email);
    }

    [Fact]
    public void Constructor_ShouldNotTreatHumanReplyMentioningAnErrorAsBounce()
    {
        var body = "Oi, recebi um erro 550 no meu sistema ontem. Pode me ajudar?";

        var bounce = new MailBounce("Dúvida", body);

        Assert.False(bounce.IsBounce);
    }

    [Fact]
    public void BounceMessageContentExtractor_ShouldReadStructuredMimeDsn()
    {
        const string rawMessage = """
            From: MAILER-DAEMON <mailer-daemon@example.org>
            To: bounces@sharebook.com.br
            Subject: Delivery Status Notification (Failure)
            MIME-Version: 1.0
            Content-Type: multipart/report; report-type=delivery-status; boundary=report

            --report
            Content-Type: text/plain; charset=utf-8

            Your message could not be delivered.

            --report
            Content-Type: message/delivery-status

            Reporting-MTA: dns; example.org

            Final-Recipient: rfc822; structured@example.com
            Action: failed
            Status: 5.1.1
            Diagnostic-Code: smtp; 550 5.1.1 User unknown
            --report--
            """;

        var message = MimeMessage.Load(new MemoryStream(Encoding.UTF8.GetBytes(rawMessage)));
        var content = BounceMessageContentExtractor.Extract(message);
        var bounce = new MailBounce(message.Subject, content);

        Assert.Contains("Final-Recipient: rfc822; structured@example.com", content);
        Assert.True(bounce.IsBounce);
        Assert.False(bounce.IsSoft);
        Assert.Equal("550", bounce.ErrorCode);
        Assert.Equal("structured@example.com", bounce.Email);
    }
}
