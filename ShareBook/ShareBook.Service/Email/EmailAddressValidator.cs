using MimeKit;
using System.Linq;

namespace ShareBook.Service;

public static class EmailAddressValidator
{
    public static bool IsValid(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var normalizedEmail = email.Trim();
        if (normalizedEmail.Count(c => c == '@') != 1)
            return false;

        return MailboxAddress.TryParse(normalizedEmail, out var mailboxAddress)
            && string.Equals(mailboxAddress.Address, normalizedEmail, System.StringComparison.Ordinal);
    }
}
