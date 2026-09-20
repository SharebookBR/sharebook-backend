namespace ShareBook.Service;

public class EmailSettings
{
    public string Sender { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int Port { get; set; }
    public bool UseSSL { get; set; }
    public string SmtpHostName { get; set; } = string.Empty;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public bool? SmtpUseSSL { get; set; }
    public string ReturnPath { get; set; } = string.Empty;
    public string ImapHostName { get; set; } = string.Empty;
    public string ImapUsername { get; set; } = string.Empty;
    public string ImapPassword { get; set; } = string.Empty;
    public bool? ImapUseSSL { get; set; }
    public int ImapPort { get; set; }
    public string BounceFolder { get; set; } = string.Empty;

    public string EffectiveSmtpHostName => string.IsNullOrWhiteSpace(SmtpHostName) ? HostName : SmtpHostName;
    public string EffectiveSmtpUsername => string.IsNullOrWhiteSpace(SmtpUsername) ? Username : SmtpUsername;
    public string EffectiveSmtpPassword => string.IsNullOrWhiteSpace(SmtpPassword) ? Password : SmtpPassword;
    public int EffectiveSmtpPort => SmtpPort > 0 ? SmtpPort : Port;
    public bool EffectiveSmtpUseSSL => SmtpUseSSL ?? UseSSL;
    public string EffectiveReturnPath => string.IsNullOrWhiteSpace(ReturnPath) ? Sender : ReturnPath;
    public string EffectiveImapHostName => string.IsNullOrWhiteSpace(ImapHostName) ? HostName : ImapHostName;
    public string EffectiveImapUsername => string.IsNullOrWhiteSpace(ImapUsername) ? Username : ImapUsername;
    public string EffectiveImapPassword => string.IsNullOrWhiteSpace(ImapPassword) ? Password : ImapPassword;
    public bool EffectiveImapUseSSL => ImapUseSSL ?? UseSSL;
}
