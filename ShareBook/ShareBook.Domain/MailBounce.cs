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
        // tenta extrair o email de destino original do corpo do email
        string pattern = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
        Match match = Regex.Match(Body, pattern);
        Email = match.Success ? match.Value : "";

        // Check if email body contains an error code
        var errorCodeMatch = Regex.Match(Body, @"Remote Server returned: '(\d{3})");

        if (errorCodeMatch.Success)
        {
            IsBounce = true;
            ErrorCode = errorCodeMatch.Groups.Count == 2 ? errorCodeMatch.Groups[1].Value : "";

            if (ErrorCode.StartsWith("4"))
            {
                // Soft bounce
                IsSoft = true;
            }
        }
    }

}