using ShareBook.Domain.Common;
using System.Text.RegularExpressions;

namespace ShareBook.Domain;

public class MailBounce: BaseEntity
{
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
        
        // tenta extrair o email de destino original do corpo do email
        string pattern = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
        Match match = Regex.Match(Body, pattern);
        Email = match.Success ? match.Value : "";

        var errorCodeMatch = Regex.Match(Body, @"Remote Server returned:\s*'(?<code>[45]\d{2})");

        if (errorCodeMatch.Success)
        {
            IsBounce = true;
            ErrorCode = errorCodeMatch.Groups["code"].Value;

            if (ErrorCode.StartsWith("4"))
            {
                // Soft bounce
                IsSoft = true;
            }

            return;
        }

        errorCodeMatch = Regex.Match(Body, @"\bwith code (?<code>[45]\d{2})\b", RegexOptions.IgnoreCase);

        if (errorCodeMatch.Success)
        {
            IsBounce = true;
            ErrorCode = errorCodeMatch.Groups["code"].Value;
            IsSoft = ErrorCode.StartsWith("4");
        }
    }

}
