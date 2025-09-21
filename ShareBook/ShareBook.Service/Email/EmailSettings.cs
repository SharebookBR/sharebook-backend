namespace ShareBook.Service;

public class EmailSettings
{
    public string Sender { get; set; }
    public string HostName { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public int Port { get; set; }
    public bool UseSSL { get; set; }
    public int ImapPort { get; set; }
    public string BounceFolder { get; set; }
}
