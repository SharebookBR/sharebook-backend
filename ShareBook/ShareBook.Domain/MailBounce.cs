using ShareBook.Domain.Common;
using System.Text.RegularExpressions;

namespace ShareBook.Domain;

public class MailBounce: BaseEntity
{
    private static readonly Regex EmailRegex = new(
        @"\b(?<email>[\w.+-]+@[\w.-]+\.[A-Za-z]{2,})\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex RecipientRegex = new(
        @"(?im)^\s*(?:Final-Recipient|Original-Recipient|Recipient|Destinatário(?:\s+final)?)\s*:\s*(?:rfc822\s*;\s*)?<?(?<email>[\w.+-]+@[\w.-]+\.[A-Za-z]{2,})>?",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex SmtpCodeRegex = new(
        @"(?ix)
            (?:
                remote\s+server\s+returned\s*:?[\s']*
              | with\s+code\s+
              | diagnostic-code\s*:\s*(?:smtp\s*;\s*)?
              | smtp\s+(?:error|reply|status)\s*[:=]\s*
            )
            (?<code>[45]\d{2})\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex EnhancedStatusRegex = new(
        @"(?im)^\s*Status\s*:\s*(?<status>[45])\.\d{1,3}\.\d{1,3}\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex BounceEvidenceRegex = new(
        @"(?ix)
            \b(?:
                final-recipient|original-recipient|diagnostic-code|action\s*:\s*(?:failed|delayed)
              | delivery\s+status\s+notification|failed\s+to\s+deliver|could\s+not\s+be\s+delivered
              | undeliverable|returned\s+mail|recipient\s+(?:address\s+)?rejected
              | no\s+mx\s+record\s+found|connection\s+timed\s+out|i/o\s+error
              | não\s+(?:foi\s+possível\s+)?entreg(?:ar|ue)|falha\s+(?:na|de)\s+entrega
              | endereço\s+(?:de\s+e-?mail\s+)?(?:não\s+)?(?:existe|encontrado|localizado)
            )\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string? Email { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public string? ErrorCode { get; set; }
    public bool IsSoft { get; set; } = false;
    public bool IsBounce { get; set; } = false;

    public MailBounce(string subject, string body)
    {
        Subject = subject;
        Body = body;

        ExtractFromBody();
    }

    private void ExtractFromBody()
    {
        if (string.IsNullOrEmpty(Body)) return;

        // DSNs frequentemente carregam o remetente original no texto anexado. Priorizamos
        // os campos de destinatário do relatório para não suprimir a pessoa errada.
        var recipient = RecipientRegex.Match(Body);
        var firstEmail = EmailRegex.Match(Body);
        Email = recipient.Success
            ? recipient.Groups["email"].Value
            : firstEmail.Success ? firstEmail.Groups["email"].Value : string.Empty;

        var smtpCode = SmtpCodeRegex.Match(Body);
        if (smtpCode.Success)
        {
            MarkBounce(smtpCode.Groups["code"].Value);
            return;
        }

        // RFC 3464: Status: 5.1.1 / 4.2.0. O status estruturado é evidência
        // suficiente de bounce mesmo quando o servidor não inclui um código SMTP.
        var enhancedStatus = EnhancedStatusRegex.Match(Body);
        if (enhancedStatus.Success
            && (recipient.Success || Regex.IsMatch(Body, @"(?im)^\s*Action\s*:\s*(?:failed|delayed)\b")))
        {
            MarkBounce(enhancedStatus.Groups["status"].Value == "4" ? "450" : "550");
            return;
        }

        if (!BounceEvidenceRegex.IsMatch($"{Subject}\n{Body}"))
            return;

        if (Regex.IsMatch(Body, @"\bno\s+mx\s+record\s+found\b", RegexOptions.IgnoreCase)
            || Regex.IsMatch(Body, @"\b(?:não\s+existe|endereço\s+inválido)\b", RegexOptions.IgnoreCase))
        {
            MarkBounce("550");
            return;
        }

        if (Regex.IsMatch(Body, @"\b(connection\s+timed\s+out|i/o\s+error|temporar(?:y|iamente)|atraso\s+na\s+entrega)\b", RegexOptions.IgnoreCase))
            MarkBounce("421");
    }

    private void MarkBounce(string code)
    {
        IsBounce = true;
        ErrorCode = code;
        IsSoft = code.StartsWith("4", StringComparison.Ordinal);
    }

}
