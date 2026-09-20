

namespace ShareBook.Infra.CrossCutting.Identity;


public class TokenConfigurations
{
    // Populado depois da construção via ConfigureFromConfigurationOptions (ver JWTConfig),
    // não em um único object initializer — por isso não dá pra usar "required" aqui.
    public string Audience { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public int Seconds { get; set; }
}
