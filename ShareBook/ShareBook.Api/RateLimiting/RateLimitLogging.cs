namespace ShareBook.Api.RateLimiting;

public static class RateLimitLogging
{
    // Propriedade estrutural usada como marcador: só eventos com essa propriedade
    // são espelhados na tabela "Logs" do Postgres (ver Program.cs).
    public const string CategoryProperty = "LogsCategory";

    public const string EBookDownloadCategory = "EBookDownload.RateLimit";
}
