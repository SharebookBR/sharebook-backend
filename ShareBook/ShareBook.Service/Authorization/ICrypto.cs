namespace ShareBook.Service.Authorization;

public interface ICrypto
{
    public string Encrypt(string input);
    public string Decrypt(string input);
}
