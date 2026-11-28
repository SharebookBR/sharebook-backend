using MimeKit;
using System;

namespace ShareBook.Service;

/// <summary>
/// Identifica autorespostas automáticas que não são DSNs e não devem permanecer
/// na caixa exclusiva de bounces.
/// </summary>
public static class AutoReplyMessageDetector
{
    public static bool IsAutoReply(MimeMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        return HeaderStartsWith(message.Headers["Auto-Submitted"], "auto-replied")
            || HeaderStartsWith(message.Headers["X-Autoreply"], "yes");
    }

    private static bool HeaderStartsWith(string? value, string expected)
        => !string.IsNullOrWhiteSpace(value)
            && value.TrimStart().StartsWith(expected, StringComparison.OrdinalIgnoreCase);
}
