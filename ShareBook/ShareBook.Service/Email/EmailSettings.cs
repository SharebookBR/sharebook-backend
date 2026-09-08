namespace ShareBook.Service;

public class EmailSettings
{
    public string Sender { get; set; }
    public string HostName { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public int Port { get; set; }
    public bool UseSSL { get; set; }
    public string SmtpHostName { get; set; }
    public string SmtpUsername { get; set; }
    public string SmtpPassword { get; set; }
    public int SmtpPort { get; set; }
    public bool? SmtpUseSSL { get; set; }
    public string ReturnPath { get; set; }
    public string ImapHostName { get; set; }
    public string ImapUsername { get; set; }
    public string ImapPassword { get; set; }
    public bool? ImapUseSSL { get; set; }
    public int ImapPort { get; set; }
    public string BounceFolder { get; set; }

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
